namespace Sinistros.Application.Events;

public sealed record SinistroCriadoIntegrationEvent(
    Guid SinistroId,
    string NumeroSinistro,
    string PlacaVeiculo,
    string NumeroApolice,
    DateTime DataOcorrenciaUtc,
    DateTime OcorreuEmUtc);

public sealed record VistoriaSolicitadaIntegrationEvent(
    Guid SinistroId,
    string NumeroSinistro,
    string PlacaVeiculo,
    DateTime SolicitadoEmUtc);

public sealed record VistoriaConcluidaIntegrationEvent(
    Guid VistoriaId,
    Guid SinistroId,
    string PeritoId,
    decimal CustoTotalOrcado,
    int QuantidadeAvarias,
    int QuantidadeFotos,
    DateTime ConcluidoEmUtc);

public sealed record SinistroAprovadoIntegrationEvent(
    Guid SinistroId,
    string NumeroSinistro,
    string UsuarioAprovador,
    string NivelAlcada,
    decimal ValorIndenizacao,
    DateTime AprovadoEmUtc);

public sealed record SinistroLiquidadoIntegrationEvent(
    Guid SinistroId,
    string NumeroSinistro,
    decimal ValorLiquidado,
    DateTime LiquidadoEmUtc);
