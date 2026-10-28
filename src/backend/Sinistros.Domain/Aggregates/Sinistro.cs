using Sinistros.Domain.Common;
using Sinistros.Domain.DomainServices;
using Sinistros.Domain.Entities;
using Sinistros.Domain.Enums;
using Sinistros.Domain.Events;
using Sinistros.Domain.Exceptions;
using Sinistros.Domain.ValueObjects;

namespace Sinistros.Domain.Aggregates;

/// <summary>
/// Raiz de Agregação do Contexto de Sinistros e Regulação.
/// Gerencia o ciclo de vida, esteira de alçadas e conformidade do processo indenizatório.
/// </summary>
public sealed class Sinistro : AggregateRoot<Guid>
{
    private readonly List<RegistroAuditoria> _auditoria = [];

    public string NumeroSinistro { get; private set; } = null!;
    public DateTime DataAberturaUtc { get; private set; }
    public DateTime DataOcorrenciaUtc { get; private set; }
    public PlacaVeiculo PlacaVeiculo { get; private set; } = null!;
    public NumeroApolice NumeroApolice { get; private set; } = null!;
    public ValorMonetario Franquia { get; private set; }
    public ValorMonetario ValorFipeReferencia { get; private set; }
    public StatusSinistro Status { get; private set; }

    public ParecerRegulacao? Parecer { get; private set; }
    public IReadOnlyCollection<RegistroAuditoria> Auditoria => _auditoria.AsReadOnly();

    private Sinistro() : base() { }

    public static Sinistro Abrir(
        string numeroSinistro,
        PlacaVeiculo placa,
        NumeroApolice apolice,
        ValorMonetario franquia,
        ValorMonetario valorFipe,
        DateTime dataOcorrenciaUtc,
        string usuarioCriador,
        string? ipOrigem = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(numeroSinistro);
        ArgumentException.ThrowIfNullOrWhiteSpace(usuarioCriador);

        if (dataOcorrenciaUtc > DateTime.UtcNow)
            throw new RegraNegocioException("DATA_OCORRENCIA_FUTURA", "A data da ocorrência do sinistro não pode ser futura.");

        var sinistro = new Sinistro
        {
            Id = Guid.NewGuid(),
            NumeroSinistro = numeroSinistro.Trim().ToUpperInvariant(),
            DataAberturaUtc = DateTime.UtcNow,
            DataOcorrenciaUtc = dataOcorrenciaUtc,
            PlacaVeiculo = placa ?? throw new ArgumentNullException(nameof(placa)),
            NumeroApolice = apolice ?? throw new ArgumentNullException(nameof(apolice)),
            Franquia = franquia,
            ValorFipeReferencia = valorFipe,
            Status = StatusSinistro.Aberto
        };

        sinistro.RegistrarEventoAuditoria(usuarioCriador, "AberturaSinistro", $"Sinistro aberto com FIPE de {valorFipe} e franquia de {franquia}.", ipOrigem);
        sinistro.RaiseDomainEvent(new SinistroAbertoDomainEvent(sinistro.Id, sinistro.NumeroSinistro, placa.Valor, apolice.Codigo, dataOcorrenciaUtc));

        return sinistro;
    }

    public void SolicitarVistoria(string usuario, string? ipOrigem = null)
    {
        if (Status != StatusSinistro.Aberto)
            throw new RegraNegocioException("STATUS_INVALIDO_PARA_VISTORIA", $"Não é permitido solicitar vistoria para sinistro com status '{Status}'.");

        Status = StatusSinistro.AguardandoVistoria;
        RegistrarEventoAuditoria(usuario, "SolicitacaoVistoria", "Ordem de vistoria técnica emitida com sucesso.", ipOrigem);
        RaiseDomainEvent(new VistoriaSolicitadaDomainEvent(Id, NumeroSinistro, PlacaVeiculo.Valor));
    }

    public void ProcessarLaudoVistoria(
        ValorMonetario custoTotalOrcado,
        MotorCalculoIndenizacaoService motorCalculo,
        string usuario,
        string? observacoes = null,
        string? ipOrigem = null)
    {
        ArgumentNullException.ThrowIfNull(motorCalculo);

        if (Status != StatusSinistro.AguardandoVistoria && Status != StatusSinistro.VistoriaConcluida)
            throw new RegraNegocioException("STATUS_INVALIDO_REGULACAO", $"Não é possível processar regulação para sinistro no status '{Status}'.");

        Parecer = motorCalculo.CalcularParecer(Id, custoTotalOrcado, ValorFipeReferencia, Franquia, observacoes);

        if (Parecer.NivelAlcadaRequerido == NivelAlcada.Operacional)
        {
            // Auto-aprovação dentro da alçada operacional
            Status = StatusSinistro.Aprovado;
            RegistrarEventoAuditoria(
                usuario,
                "AutoAprovacao",
                $"Sinistro auto-aprovado no limite operacional ({Parecer.TipoPerda}). Valor líquido: {Parecer.ValorLiquidoIndenizar}.",
                ipOrigem);

            RaiseDomainEvent(new SinistroAprovadoDomainEvent(
                Id,
                "SISTEMA_AUTO_APROVACAO",
                NivelAlcada.Operacional,
                Parecer.ValorLiquidoIndenizar.Quantia,
                "Auto-aprovação baseada em regras de conformidade e valor dentro do teto operacional."));
        }
        else
        {
            Status = StatusSinistro.AguardandoAprovacaoAlcada;
            RegistrarEventoAuditoria(
                usuario,
                "ParecerCalculado",
                $"Parecer gerado ({Parecer.TipoPerda}). Custo orçado: {custoTotalOrcado}. Requer aprovação de nível {Parecer.NivelAlcadaRequerido}.",
                ipOrigem);

            RaiseDomainEvent(new ParecerRegulacaoCalculadoDomainEvent(
                Id,
                Parecer.TipoPerda,
                Parecer.CustoTotalOrcado.Quantia,
                Parecer.ValorFipeReferencia.Quantia,
                Parecer.FranquiaAplicada.Quantia,
                Parecer.ValorLiquidoIndenizar.Quantia,
                Parecer.NivelAlcadaRequerido));
        }
    }

    public void Aprovar(
        string usuarioAprovador,
        NivelAlcada nivelUsuario,
        string justificativa,
        AlcadaAprovacaoService alcadaService,
        string? ipOrigem = null)
    {
        ArgumentNullException.ThrowIfNull(alcadaService);
        ArgumentException.ThrowIfNullOrWhiteSpace(usuarioAprovador);
        ArgumentException.ThrowIfNullOrWhiteSpace(justificativa);

        if (Status != StatusSinistro.AguardandoAprovacaoAlcada)
            throw new RegraNegocioException("STATUS_INVALIDO_APROVACAO", $"Sinistro não está aguardando aprovação de alçada (Status atual: {Status}).");

        if (Parecer is null)
            throw new RegraNegocioException("PARECER_AUSENTE", "Não é possível aprovar um sinistro sem parecer técnico de regulação.");

        if (!alcadaService.PossuiAlcadaSuficiente(nivelUsuario, Parecer.NivelAlcadaRequerido))
        {
            throw new AlcadaInsuficienteException(
                nivelUsuario.ToString(),
                Parecer.ValorLiquidoIndenizar.Quantia,
                nivelUsuario == NivelAlcada.Operacional ? AlcadaAprovacaoService.LimiteOperacional : AlcadaAprovacaoService.LimiteSupervisao);
        }

        Status = StatusSinistro.Aprovado;
        RegistrarEventoAuditoria(
            usuarioAprovador,
            "AprovacaoAlcada",
            $"Aprovado manualmente por {usuarioAprovador} ({nivelUsuario}). Justificativa: {justificativa}",
            ipOrigem);

        RaiseDomainEvent(new SinistroAprovadoDomainEvent(
            Id,
            usuarioAprovador,
            nivelUsuario,
            Parecer.ValorLiquidoIndenizar.Quantia,
            justificativa));
    }

    public void Rejeitar(string usuarioRejeitador, string motivo, string? ipOrigem = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(usuarioRejeitador);
        ArgumentException.ThrowIfNullOrWhiteSpace(motivo);

        if (Status == StatusSinistro.Liquidado)
            throw new RegraNegocioException("SINISTRO_JA_LIQUIDADO", "Não é permitido rejeitar um sinistro já liquidado financeiramente.");

        Status = StatusSinistro.Rejeitado;
        RegistrarEventoAuditoria(usuarioRejeitador, "RejeicaoSinistro", $"Sinistro rejeitado por {usuarioRejeitador}. Motivo: {motivo}", ipOrigem);
        RaiseDomainEvent(new SinistroRejeitadoDomainEvent(Id, usuarioRejeitador, motivo));
    }

    public void Liquidar(string usuario, string? ipOrigem = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(usuario);

        if (Status != StatusSinistro.Aprovado)
            throw new RegraNegocioException("STATUS_INVALIDO_LIQUIDACAO", $"Apenas sinistros aprovados podem ser liquidados (Status atual: {Status}).");

        if (Parecer is null)
            throw new RegraNegocioException("PARECER_AUSENTE", "Parecer de regulação ausente para liquidação.");

        Status = StatusSinistro.Liquidado;
        RegistrarEventoAuditoria(usuario, "LiquidacaoFinanceira", $"Sinistro liquidado financeiramente no valor de {Parecer.ValorLiquidoIndenizar}.", ipOrigem);
        RaiseDomainEvent(new SinistroLiquidadoDomainEvent(Id, Parecer.ValorLiquidoIndenizar.Quantia, DateTime.UtcNow));
    }

    private void RegistrarEventoAuditoria(string usuario, string acao, string detalhes, string? ipOrigem = null)
    {
        _auditoria.Add(new RegistroAuditoria(Id, usuario, acao, detalhes, ipOrigem));
    }
}
