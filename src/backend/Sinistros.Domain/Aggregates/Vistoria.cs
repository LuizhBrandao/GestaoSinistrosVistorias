using Sinistros.Domain.Common;
using Sinistros.Domain.Entities;
using Sinistros.Domain.Enums;
using Sinistros.Domain.Events;
using Sinistros.Domain.Exceptions;
using Sinistros.Domain.ValueObjects;

namespace Sinistros.Domain.Aggregates;

/// <summary>
/// Raiz de Agregação do Contexto de Vistorias e Evidências Técnicas.
/// Controla o checklist de avarias por peças e o acervo probatório de fotos.
/// </summary>
public sealed class Vistoria : AggregateRoot<Guid>
{
    private readonly List<ItemAvaria> _avarias = [];
    private readonly List<FotoEvidencia> _evidencias = [];

    public Guid SinistroId { get; private set; }
    public string PeritoId { get; private set; } = null!;
    public DateTime DataAgendamentoUtc { get; private set; }
    public DateTime? DataConclusaoUtc { get; private set; }
    public StatusVistoria Status { get; private set; }
    public string? ObservacoesPerito { get; private set; }

    public IReadOnlyCollection<ItemAvaria> Avarias => _avarias.AsReadOnly();
    public IReadOnlyCollection<FotoEvidencia> Evidencias => _evidencias.AsReadOnly();

    public ValorMonetario CustoTotalOrcado => _avarias.Aggregate(
        ValorMonetario.Zero,
        (acumulador, avaria) => acumulador.Somar(avaria.CustoTotal));

    private Vistoria() : base() { }

    public static Vistoria Agendar(Guid sinistroId, string peritoId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(peritoId);

        return new Vistoria
        {
            Id = Guid.NewGuid(),
            SinistroId = sinistroId,
            PeritoId = peritoId.Trim(),
            DataAgendamentoUtc = DateTime.UtcNow,
            Status = StatusVistoria.Agendada
        };
    }

    public void Iniciar()
    {
        if (Status != StatusVistoria.Agendada)
            throw new RegraNegocioException("STATUS_INVALIDO_INICIO_VISTORIA", $"Vistoria não pode ser iniciada no status '{Status}'.");

        Status = StatusVistoria.EmAndamento;
    }

    public void AdicionarAvaria(string peca, TipoAcaoAvaria tipoAcao, ValorMonetario custoMaoDeObra, ValorMonetario custoPecas)
    {
        if (Status == StatusVistoria.Concluida || Status == StatusVistoria.Cancelada)
            throw new RegraNegocioException("VISTORIA_FINALIZADA", "Não é permitido adicionar avarias a uma vistoria finalizada ou cancelada.");

        if (Status == StatusVistoria.Agendada)
            Status = StatusVistoria.EmAndamento;

        _avarias.Add(new ItemAvaria(Id, peca, tipoAcao, custoMaoDeObra, custoPecas));
    }

    public void RegistrarEvidencia(
        string urlOriginal,
        string urlComprimida,
        string hashArquivoSha256,
        CoordenadasGps? coordenadasGps = null,
        DateTime? dataCapturaExif = null)
    {
        if (Status == StatusVistoria.Concluida || Status == StatusVistoria.Cancelada)
            throw new RegraNegocioException("VISTORIA_FINALIZADA", "Não é permitido anexar fotos a uma vistoria finalizada.");

        _evidencias.Add(new FotoEvidencia(Id, urlOriginal, urlComprimida, hashArquivoSha256, coordenadasGps, dataCapturaExif));
    }

    public void Concluir(string observacoesPerito)
    {
        if (Status == StatusVistoria.Concluida)
            throw new RegraNegocioException("JA_CONCLUIDA", "A vistoria já foi concluída anteriormente.");

        // Regra de Integridade Pericial: Não é permitida a emissão de laudo sem registros fotográficos
        if (_evidencias.Count == 0)
            throw new RegraNegocioException("FOTOS_OBRIGATORIAS", "A vistoria requer no mínimo uma evidência fotográfica anexada para ser concluída.");

        Status = StatusVistoria.Concluida;
        DataConclusaoUtc = DateTime.UtcNow;
        ObservacoesPerito = observacoesPerito?.Trim();

        RaiseDomainEvent(new VistoriaConcluidaDomainEvent(
            Id,
            SinistroId,
            PeritoId,
            CustoTotalOrcado.Quantia,
            _avarias.Count,
            _evidencias.Count));
    }

    public void Cancelar(string motivo)
    {
        if (Status == StatusVistoria.Concluida)
            throw new RegraNegocioException("NAO_PODE_CANCELAR_CONCLUIDA", "Uma vistoria concluída não pode ser cancelada.");

        Status = StatusVistoria.Cancelada;
        ObservacoesPerito = $"Cancelada: {motivo}";
    }
}
