using FluentAssertions;
using NSubstitute;
using Sinistros.Application.Commands.AprovarSinistro;
using Sinistros.Application.Common;
using Sinistros.Application.Events;
using Sinistros.Domain.Aggregates;
using Sinistros.Domain.DomainServices;
using Sinistros.Domain.Enums;
using Sinistros.Domain.Repositories;
using Sinistros.Domain.ValueObjects;
using Xunit;

namespace Sinistros.Application.Tests;

public class AprovarSinistroCommandHandlerTests
{
    private readonly ISinistroRepository _sinistroRepository = Substitute.For<ISinistroRepository>();
    private readonly AlcadaAprovacaoService _alcadaService = new();
    private readonly MotorCalculoIndenizacaoService _motorCalculo;
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IMessageBus _messageBus = Substitute.For<IMessageBus>();
    private readonly AprovarSinistroCommandHandler _handler;

    public AprovarSinistroCommandHandlerTests()
    {
        _motorCalculo = new MotorCalculoIndenizacaoService(_alcadaService);
        _handler = new AprovarSinistroCommandHandler(_sinistroRepository, _alcadaService, _unitOfWork, _messageBus);
    }

    [Fact]
    public async Task Handle_QuandoSupervisorAprovaSinistroDeAlcadaSupervisao_DeveSucesso()
    {
        // Arrange
        var sinistro = Sinistro.Abrir(
            "SIN-2026-200",
            new PlacaVeiculo("ABC1D23"),
            new NumeroApolice("APOL-123456"),
            ValorMonetario.De(2_000m),
            ValorMonetario.De(80_000m),
            DateTime.UtcNow.AddDays(-1),
            "usuario.teste");

        sinistro.SolicitarVistoria("usuario.teste");
        // Orçamento de R$ 25.000 -> Líquido R$ 23.000 (Supervisão)
        sinistro.ProcessarLaudoVistoria(ValorMonetario.De(25_000m), _motorCalculo, "sistema");

        _sinistroRepository.ObterPorIdAsync(sinistro.Id, Arg.Any<CancellationToken>())
            .Returns(sinistro);

        var command = new AprovarSinistroCommand(
            sinistro.Id,
            "supervisor.joao",
            NivelAlcada.Supervisao,
            "Parecer técnico devidamente verificado.");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        sinistro.Status.Should().Be(StatusSinistro.Aprovado);
        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
        await _messageBus.Received(1).PublishAsync(
            Arg.Is<SinistroAprovadoIntegrationEvent>(e => e.SinistroId == sinistro.Id && e.UsuarioAprovador == "supervisor.joao"),
            "sinistro.evento.aprovado",
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_QuandoAnalistaTentaAprovarSinistroDeAlcadaSupervisao_DeveRetornarFalhaAlcada()
    {
        // Arrange
        var sinistro = Sinistro.Abrir(
            "SIN-2026-200",
            new PlacaVeiculo("ABC1D23"),
            new NumeroApolice("APOL-123456"),
            ValorMonetario.De(2_000m),
            ValorMonetario.De(80_000m),
            DateTime.UtcNow.AddDays(-1),
            "usuario.teste");

        sinistro.SolicitarVistoria("usuario.teste");
        sinistro.ProcessarLaudoVistoria(ValorMonetario.De(25_000m), _motorCalculo, "sistema");

        _sinistroRepository.ObterPorIdAsync(sinistro.Id, Arg.Any<CancellationToken>())
            .Returns(sinistro);

        var command = new AprovarSinistroCommand(
            sinistro.Id,
            "analista.carlos",
            NivelAlcada.Operacional,
            "Tentativa de aprovação direta sem autoridade.");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.ErrorCode.Should().Be("ALCADA_INSUFICIENTE");
    }
}
