using Sinistros.Domain.Enums;

namespace Sinistros.Application.DTOs;

public sealed record SinistroResumoDto(
    Guid Id,
    string NumeroSinistro,
    string PlacaVeiculo,
    string NumeroApolice,
    StatusSinistro Status,
    decimal Franquia,
    decimal ValorFipeReferencia,
    DateTime DataOcorrenciaUtc,
    DateTime DataAberturaUtc);

public sealed record SinistroDetalhesDto(
    Guid Id,
    string NumeroSinistro,
    string PlacaVeiculo,
    string NumeroApolice,
    StatusSinistro Status,
    decimal Franquia,
    decimal ValorFipeReferencia,
    DateTime DataOcorrenciaUtc,
    DateTime DataAberturaUtc,
    ParecerRegulacaoDto? Parecer,
    IReadOnlyList<RegistroAuditoriaDto> Auditoria);

public sealed record ParecerRegulacaoDto(
    TipoPerda TipoPerda,
    decimal CustoTotalOrcado,
    decimal ValorFipeReferencia,
    decimal FranquiaAplicada,
    decimal ValorLiquidoIndenizar,
    NivelAlcada NivelAlcadaRequerido,
    DateTime DataCalculoUtc,
    string? Observacoes);

public sealed record RegistroAuditoriaDto(
    Guid Id,
    DateTime DataHoraUtc,
    string Usuario,
    string Acao,
    string Detalhes,
    string? IpOrigem);

public sealed record VistoriaDto(
    Guid Id,
    Guid SinistroId,
    string PeritoId,
    StatusVistoria Status,
    DateTime DataAgendamentoUtc,
    DateTime? DataConclusaoUtc,
    decimal CustoTotalOrcado,
    string? ObservacoesPerito,
    IReadOnlyList<ItemAvariaDto> Avarias,
    IReadOnlyList<FotoEvidenciaDto> Evidencias);

public sealed record ItemAvariaDto(
    Guid Id,
    string Peca,
    TipoAcaoAvaria TipoAcao,
    decimal CustoMaoDeObra,
    decimal CustoPecas,
    decimal CustoTotal);

public sealed record FotoEvidenciaDto(
    Guid Id,
    string UrlOriginal,
    string UrlComprimida,
    string HashArquivoSha256,
    double? Latitude,
    double? Longitude,
    DateTime? DataCapturaExif,
    DateTime DataUploadUtc);
