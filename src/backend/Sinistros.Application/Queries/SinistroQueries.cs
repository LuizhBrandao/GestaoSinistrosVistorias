using MediatR;
using Sinistros.Application.Common;
using Sinistros.Application.DTOs;
using Sinistros.Domain.Repositories;

namespace Sinistros.Application.Queries;

public sealed record ObterSinistroPorIdQuery(Guid Id) : IRequest<Result<SinistroDetalhesDto>>;

public sealed record ObterSinistrosPendentesQuery() : IRequest<Result<IReadOnlyList<SinistroResumoDto>>>;

public sealed record ObterVistoriaPorSinistroIdQuery(Guid SinistroId) : IRequest<Result<VistoriaDto>>;

public sealed class SinistroQueryHandlers :
    IRequestHandler<ObterSinistroPorIdQuery, Result<SinistroDetalhesDto>>,
    IRequestHandler<ObterSinistrosPendentesQuery, Result<IReadOnlyList<SinistroResumoDto>>>,
    IRequestHandler<ObterVistoriaPorSinistroIdQuery, Result<VistoriaDto>>
{
    private readonly ISinistroRepository _sinistroRepository;
    private readonly IVistoriaRepository _vistoriaRepository;

    public SinistroQueryHandlers(
        ISinistroRepository sinistroRepository,
        IVistoriaRepository vistoriaRepository)
    {
        _sinistroRepository = sinistroRepository;
        _vistoriaRepository = vistoriaRepository;
    }

    public async Task<Result<SinistroDetalhesDto>> Handle(ObterSinistroPorIdQuery request, CancellationToken cancellationToken)
    {
        var sinistro = await _sinistroRepository.ObterPorIdAsync(request.Id, cancellationToken);
        if (sinistro is null)
            return Result<SinistroDetalhesDto>.Failure($"Sinistro '{request.Id}' não encontrado.", "SINISTRO_NAO_ENCONTRADO");

        var parecerDto = sinistro.Parecer is null ? null : new ParecerRegulacaoDto(
            sinistro.Parecer.TipoPerda,
            sinistro.Parecer.CustoTotalOrcado.Quantia,
            sinistro.Parecer.ValorFipeReferencia.Quantia,
            sinistro.Parecer.FranquiaAplicada.Quantia,
            sinistro.Parecer.ValorLiquidoIndenizar.Quantia,
            sinistro.Parecer.NivelAlcadaRequerido,
            sinistro.Parecer.DataCalculoUtc,
            sinistro.Parecer.Observacoes);

        var auditoriaDto = sinistro.Auditoria.Select(a => new RegistroAuditoriaDto(
            a.Id,
            a.DataHoraUtc,
            a.Usuario,
            a.Acao,
            a.Detalhes,
            a.IpOrigem)).ToList();

        var dto = new SinistroDetalhesDto(
            sinistro.Id,
            sinistro.NumeroSinistro,
            sinistro.PlacaVeiculo.Formatada,
            sinistro.NumeroApolice.Codigo,
            sinistro.Status,
            sinistro.Franquia.Quantia,
            sinistro.ValorFipeReferencia.Quantia,
            sinistro.DataOcorrenciaUtc,
            sinistro.DataAberturaUtc,
            parecerDto,
            auditoriaDto);

        return Result<SinistroDetalhesDto>.Success(dto);
    }

    public async Task<Result<IReadOnlyList<SinistroResumoDto>>> Handle(ObterSinistrosPendentesQuery request, CancellationToken cancellationToken)
    {
        var sinistros = await _sinistroRepository.ObterPendentesAprovacaoAsync(cancellationToken);

        var lista = sinistros.Select(s => new SinistroResumoDto(
            s.Id,
            s.NumeroSinistro,
            s.PlacaVeiculo.Formatada,
            s.NumeroApolice.Codigo,
            s.Status,
            s.Franquia.Quantia,
            s.ValorFipeReferencia.Quantia,
            s.DataOcorrenciaUtc,
            s.DataAberturaUtc)).ToList();

        return Result<IReadOnlyList<SinistroResumoDto>>.Success(lista);
    }

    public async Task<Result<VistoriaDto>> Handle(ObterVistoriaPorSinistroIdQuery request, CancellationToken cancellationToken)
    {
        var vistoria = await _vistoriaRepository.ObterPorSinistroIdAsync(request.SinistroId, cancellationToken);
        if (vistoria is null)
            return Result<VistoriaDto>.Failure($"Nenhuma vistoria encontrada para o sinistro '{request.SinistroId}'.", "VISTORIA_NAO_ENCONTRADA");

        var avariasDto = vistoria.Avarias.Select(a => new ItemAvariaDto(
            a.Id,
            a.Peca,
            a.TipoAcao,
            a.CustoMaoDeObra.Quantia,
            a.CustoPecas.Quantia,
            a.CustoTotal.Quantia)).ToList();

        var fotosDto = vistoria.Evidencias.Select(f => new FotoEvidenciaDto(
            f.Id,
            f.UrlOriginal,
            f.UrlComprimida,
            f.HashArquivoSha256,
            f.CoordenadasGps?.Latitude,
            f.CoordenadasGps?.Longitude,
            f.DataCapturaExif,
            f.DataUploadUtc)).ToList();

        var dto = new VistoriaDto(
            vistoria.Id,
            vistoria.SinistroId,
            vistoria.PeritoId,
            vistoria.Status,
            vistoria.DataAgendamentoUtc,
            vistoria.DataConclusaoUtc,
            vistoria.CustoTotalOrcado.Quantia,
            vistoria.ObservacoesPerito,
            avariasDto,
            fotosDto);

        return Result<VistoriaDto>.Success(dto);
    }
}
