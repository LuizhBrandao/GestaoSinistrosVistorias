using FluentAssertions;
using NSubstitute;
using Sinistros.Application.Commands.AbrirSinistro;
using Sinistros.Application.Common;
using Sinistros.Application.Events;
using Sinistros.Domain.Aggregates;
using Sinistros.Domain.Repositories;
using Xunit;

namespace Sinistros.Application.Tests;

public class AbrirSinistroCommandHandlerTests
{
    private readonly ISinistroRepository _sinistroRepository = Substitute.For<ISinistroRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IMessageBus _messageBus = Substitute.For<IMessageBus>();
    private readonly AbrirSinistroCommandHandler _handler;

    public AbrirSinistroCommandHandlerTests()
    {
        _handler = new AbrirSinistroCommandHandler(_sinistroRepository, _unitOfWork, _messageBus);
    }

    [Fact]
    public async Task Handle_ComDadosValidos_DevePersistirEEnviarEventoIntegration()
    {
        // Arrange
        var command = new AbrirSinistroCommand(
            "SIN-2026-100",
            "ABC1D23",
            "APOL-999888",
            2_000m,
            75_000m,
            DateTime.UtcNow.AddHours(-2),
            "usuario.operador");

        _sinistroRepository.ObterPorNumeroAsync(command.NumeroSinistro, Arg.Any<CancellationToken>())
            .Returns((Sinistro?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        await _sinistroRepository.Received(1).AdicionarAsync(Arg.Any<Sinistro>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
        await _messageBus.Received(1).PublishAsync(
            Arg.Is<SinistroCriadoIntegrationEvent>(e => e.NumeroSinistro == "SIN-2026-100"),
            "sinistro.evento.criado",
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ComSinistroDuplicado_DeveRetornarFalha()
    {
        // Arrange
        var command = new AbrirSinistroCommand(
            "SIN-2026-100",
            "ABC1D23",
            "APOL-999888",
            2_000m,
            75_000m,
            DateTime.UtcNow.AddHours(-2),
            "usuario.operador");

        var existente = Sinistro.Abrir(
            command.NumeroSinistro,
            new Domain.ValueObjects.PlacaVeiculo("ABC1D23"),
            new Domain.ValueObjects.NumeroApolice("APOL-999888"),
            Domain.ValueObjects.ValorMonetario.De(2_000m),
            Domain.ValueObjects.ValorMonetario.De(75_000m),
            DateTime.UtcNow.AddDays(-1),
            "outro.usuario");

        _sinistroRepository.ObterPorNumeroAsync(command.NumeroSinistro, Arg.Any<CancellationToken>())
            .Returns(existente);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.ErrorCode.Should().Be("SINISTRO_DUPLICADO");
        await _sinistroRepository.DidNotReceive().AdicionarAsync(Arg.Any<Sinistro>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ComDadosInvalidos_DeveRetornarFalhaValidacao()
    {
        // Arrange: Placa vazia e FIPE zerada
        var command = new AbrirSinistroCommand(
            "SIN-2026-100",
            "",
            "APOL-999888",
            2_000m,
            0m,
            DateTime.UtcNow,
            "usuario.operador");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.ErrorCode.Should().Be("VALIDACAO_FALHOU");
    }
}
