using Sinistros.Domain.Aggregates;
using Sinistros.Domain.DomainServices;
using Sinistros.Domain.Enums;
using Sinistros.Domain.Events;
using Sinistros.Domain.Exceptions;
using Sinistros.Domain.ValueObjects;
using Xunit;

namespace Sinistros.Domain.Tests;

public class SinistroTests
{
    private readonly MotorCalculoIndenizacaoService _motorCalculo;
    private readonly AlcadaAprovacaoService _alcadaService;

    public SinistroTests()
    {
        _alcadaService = new AlcadaAprovacaoService();
        _motorCalculo = new MotorCalculoIndenizacaoService(_alcadaService);
    }

    [Fact]
    public void Abrir_ComDadosValidos_DeveCriarSinistroComStatusAbertoEEmitirEvento()
    {
        // Arrange & Act
        var sinistro = Sinistro.Abrir(
            "SIN-2026-0001",
            new PlacaVeiculo("ABC1D23"),
            new NumeroApolice("APOL-987654"),
            ValorMonetario.De(2_500m),
            ValorMonetario.De(80_000m),
            DateTime.UtcNow.AddDays(-1),
            "usuario.analista",
            "192.168.1.100");

        // Assert
        Assert.Equal(StatusSinistro.Aberto, sinistro.Status);
        Assert.Equal("SIN-2026-0001", sinistro.NumeroSinistro);
        Assert.Single(sinistro.Auditoria);
        Assert.Contains(sinistro.DomainEvents, e => e is SinistroAbertoDomainEvent);
    }

    [Fact]
    public void Abrir_ComDataOcorrenciaFutura_DeveLancarExcecao()
    {
        // Arrange & Act & Assert
        Assert.Throws<RegraNegocioException>(() => Sinistro.Abrir(
            "SIN-2026-0002",
            new PlacaVeiculo("XYZ9A87"),
            new NumeroApolice("APOL-123456"),
            ValorMonetario.De(1_000m),
            ValorMonetario.De(50_000m),
            DateTime.UtcNow.AddDays(2),
            "usuario.analista"));
    }

    [Fact]
    public void SolicitarVistoria_EmSinistroAberto_DeveAlterarStatusParaAguardandoVistoria()
    {
        // Arrange
        var sinistro = CriarSinistroPadrao();

        // Act
        sinistro.SolicitarVistoria("usuario.analista");

        // Assert
        Assert.Equal(StatusSinistro.AguardandoVistoria, sinistro.Status);
        Assert.Contains(sinistro.DomainEvents, e => e is VistoriaSolicitadaDomainEvent);
    }

    [Fact]
    public void ProcessarLaudoVistoria_ComValorDentroLimiteOperacional_DeveAutoAprovar()
    {
        // Arrange: FIPE R$ 60.000, Franquia R$ 2.000, Orçamento R$ 8.000 => Líquido R$ 6.000 (<= R$ 10.000)
        var sinistro = CriarSinistroPadrao(valorFipe: 60_000m, franquia: 2_000m);
        sinistro.SolicitarVistoria("usuario.analista");

        // Act
        sinistro.ProcessarLaudoVistoria(ValorMonetario.De(8_000m), _motorCalculo, "sistema.integracao");

        // Assert
        Assert.Equal(StatusSinistro.Aprovado, sinistro.Status);
        Assert.NotNull(sinistro.Parecer);
        Assert.Equal(6_000m, sinistro.Parecer.ValorLiquidoIndenizar.Quantia);
        Assert.Contains(sinistro.DomainEvents, e => e is SinistroAprovadoDomainEvent);
    }

    [Fact]
    public void ProcessarLaudoVistoria_ComValorAcimaLimiteOperacional_DeveAguardarAlcada()
    {
        // Arrange: FIPE R$ 80.000, Franquia R$ 3.000, Orçamento R$ 25.000 => Líquido R$ 22.000 (> R$ 10.000)
        var sinistro = CriarSinistroPadrao(valorFipe: 80_000m, franquia: 3_000m);
        sinistro.SolicitarVistoria("usuario.analista");

        // Act
        sinistro.ProcessarLaudoVistoria(ValorMonetario.De(25_000m), _motorCalculo, "sistema.integracao");

        // Assert
        Assert.Equal(StatusSinistro.AguardandoAprovacaoAlcada, sinistro.Status);
        Assert.NotNull(sinistro.Parecer);
        Assert.Equal(NivelAlcada.Supervisao, sinistro.Parecer.NivelAlcadaRequerido);
        Assert.Contains(sinistro.DomainEvents, e => e is ParecerRegulacaoCalculadoDomainEvent);
    }

    [Fact]
    public void Aprovar_ComAlcadaInsuficiente_DeveLancarAlcadaInsuficienteException()
    {
        // Arrange: Sinistro requer nível Supervisão (R$ 22.000)
        var sinistro = CriarSinistroPadrao(valorFipe: 80_000m, franquia: 3_000m);
        sinistro.SolicitarVistoria("usuario.analista");
        sinistro.ProcessarLaudoVistoria(ValorMonetario.De(25_000m), _motorCalculo, "sistema.integracao");

        // Act & Assert: Analista operacional tentando aprovar alçada de supervisão
        Assert.Throws<AlcadaInsuficienteException>(() =>
            sinistro.Aprovar("analista.junior", NivelAlcada.Operacional, "Aprovando sem alçada", _alcadaService));
    }

    [Fact]
    public void Aprovar_ComAlcadaAdequada_DeveAprovarERegistrarAuditoria()
    {
        // Arrange
        var sinistro = CriarSinistroPadrao(valorFipe: 80_000m, franquia: 3_000m);
        sinistro.SolicitarVistoria("usuario.analista");
        sinistro.ProcessarLaudoVistoria(ValorMonetario.De(25_000m), _motorCalculo, "sistema.integracao");

        // Act: Supervisor aprovando sinistro de R$ 22.000
        sinistro.Aprovar("supervisor.roberto", NivelAlcada.Supervisao, "Valores condizentes com tabela de peças", _alcadaService, "10.0.0.15");

        // Assert
        Assert.Equal(StatusSinistro.Aprovado, sinistro.Status);
        Assert.Contains(sinistro.Auditoria, a => a.Acao == "AprovacaoAlcada" && a.Usuario == "supervisor.roberto");
    }

    [Fact]
    public void Liquidar_SinistroAprovado_DeveMudarStatusParaLiquidado()
    {
        // Arrange
        var sinistro = CriarSinistroPadrao(valorFipe: 50_000m, franquia: 1_000m);
        sinistro.SolicitarVistoria("usuario.analista");
        sinistro.ProcessarLaudoVistoria(ValorMonetario.De(5_000m), _motorCalculo, "sistema"); // Auto-aprovado

        // Act
        sinistro.Liquidar("financeiro.maria", "10.0.0.20");

        // Assert
        Assert.Equal(StatusSinistro.Liquidado, sinistro.Status);
        Assert.Contains(sinistro.DomainEvents, e => e is SinistroLiquidadoDomainEvent);
    }

    private static Sinistro CriarSinistroPadrao(decimal valorFipe = 70_000m, decimal franquia = 2_000m)
    {
        return Sinistro.Abrir(
            "SIN-TEST-001",
            new PlacaVeiculo("ABC1D23"),
            new NumeroApolice("APOL-123456"),
            ValorMonetario.De(franquia),
            ValorMonetario.De(valorFipe),
            DateTime.UtcNow.AddDays(-1),
            "usuario.teste");
    }
}
