using Sinistros.Domain.Common;
using Sinistros.Domain.Enums;
using Sinistros.Domain.ValueObjects;

namespace Sinistros.Domain.Events;

public sealed record SinistroAbertoDomainEvent(
    Guid SinistroId,
    string NumeroSinistro,
    string PlacaVeiculo,
    string NumeroApolice,
    DateTime DataOcorrencia) : DomainEventBase;

public sealed record VistoriaSolicitadaDomainEvent(
    Guid SinistroId,
    string NumeroSinistro,
    string PlacaVeiculo) : DomainEventBase;

public sealed record VistoriaConcluidaDomainEvent(
    Guid VistoriaId,
    Guid SinistroId,
    string PeritoId,
    decimal CustoTotalOrcado,
    int QuantidadeAvarias,
    int QuantidadeFotos) : DomainEventBase;

public sealed record ParecerRegulacaoCalculadoDomainEvent(
    Guid SinistroId,
    TipoPerda TipoPerda,
    decimal CustoTotalReparo,
    decimal ValorFipeReferencia,
    decimal Franquia,
    decimal ValorLiquidoIndenizar,
    NivelAlcada NivelAlcadaRequerido) : DomainEventBase;

public sealed record SinistroAprovadoDomainEvent(
    Guid SinistroId,
    string UsuarioAprovador,
    NivelAlcada NivelAlcada,
    decimal ValorIndenizacao,
    string Justificativa) : DomainEventBase;

public sealed record SinistroLiquidadoDomainEvent(
    Guid SinistroId,
    decimal ValorLiquidado,
    DateTime DataLiquidacao) : DomainEventBase;

public sealed record SinistroRejeitadoDomainEvent(
    Guid SinistroId,
    string UsuarioRejeitador,
    string Motivo) : DomainEventBase;
