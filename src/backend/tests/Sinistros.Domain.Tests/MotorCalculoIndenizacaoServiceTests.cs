using Sinistros.Domain.DomainServices;
using Sinistros.Domain.Enums;
using Sinistros.Domain.ValueObjects;
using Xunit;

namespace Sinistros.Domain.Tests;

public class MotorCalculoIndenizacaoServiceTests
{
    private readonly MotorCalculoIndenizacaoService _service;

    public MotorCalculoIndenizacaoServiceTests()
    {
        _service = new MotorCalculoIndenizacaoService(new AlcadaAprovacaoService());
    }

    [Fact]
    public void CalcularParecer_QuandoOrcamentoMenorQue75PorCentoFipe_DeveSerPerdaParcialEAbaterFranquia()
    {
        // Arrange
        // FIPE: R$ 100.000, 75% = R$ 75.000
        // Orçamento de Reparo: R$ 30.000 (< 75%)
        // Franquia: R$ 3.000
        // Esperado: Perda Parcial, Valor Líquido = R$ 27.000
        var sinistroId = Guid.NewGuid();
        var fipe = ValorMonetario.De(100_000m);
        var franquia = ValorMonetario.De(3_000m);
        var orcamento = ValorMonetario.De(30_000m);

        // Act
        var parecer = _service.CalcularParecer(sinistroId, orcamento, fipe, franquia);

        // Assert
        Assert.Equal(TipoPerda.PerdaParcial, parecer.TipoPerda);
        Assert.Equal(27_000m, parecer.ValorLiquidoIndenizar.Quantia);
        Assert.Equal(NivelAlcada.Supervisao, parecer.NivelAlcadaRequerido);
    }

    [Fact]
    public void CalcularParecer_QuandoOrcamentoMaiorOuIgualA75PorCentoFipe_DeveSerPerdaTotalSemAbaterFranquia()
    {
        // Arrange
        // FIPE: R$ 100.000, 75% = R$ 75.000
        // Orçamento de Reparo: R$ 80.000 (>= 75%)
        // Franquia: R$ 3.000
        // Esperado: Perda Total, Valor Líquido = 100% da FIPE (R$ 100.000) e Alçada Gerencial
        var sinistroId = Guid.NewGuid();
        var fipe = ValorMonetario.De(100_000m);
        var franquia = ValorMonetario.De(3_000m);
        var orcamento = ValorMonetario.De(80_000m);

        // Act
        var parecer = _service.CalcularParecer(sinistroId, orcamento, fipe, franquia);

        // Assert
        Assert.Equal(TipoPerda.PerdaTotal, parecer.TipoPerda);
        Assert.Equal(100_000m, parecer.ValorLiquidoIndenizar.Quantia);
        Assert.Equal(NivelAlcada.Gerencial, parecer.NivelAlcadaRequerido);
    }
}
