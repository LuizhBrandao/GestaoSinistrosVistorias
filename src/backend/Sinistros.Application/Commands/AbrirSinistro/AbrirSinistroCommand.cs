using FluentValidation;
using MediatR;
using Sinistros.Application.Common;
using Sinistros.Application.DTOs;
using Sinistros.Application.Events;
using Sinistros.Domain.Aggregates;
using Sinistros.Domain.Repositories;
using Sinistros.Domain.ValueObjects;

namespace Sinistros.Application.Commands.AbrirSinistro;

public sealed record AbrirSinistroCommand(
    string NumeroSinistro,
    string PlacaVeiculo,
    string NumeroApolice,
    decimal Franquia,
    decimal ValorFipeReferencia,
    DateTime DataOcorrenciaUtc,
    string UsuarioCriador,
    string? IpOrigem = null) : IRequest<Result<Guid>>;

public sealed class AbrirSinistroCommandValidator : AbstractValidator<AbrirSinistroCommand>
{
    public AbrirSinistroCommandValidator()
    {
        RuleFor(x => x.NumeroSinistro)
            .NotEmpty().WithMessage("O número do sinistro é obrigatório.")
            .MaximumLength(30).WithMessage("O número do sinistro não pode exceder 30 caracteres.");

        RuleFor(x => x.PlacaVeiculo)
            .NotEmpty().WithMessage("A placa do veículo é obrigatória.");

        RuleFor(x => x.NumeroApolice)
            .NotEmpty().WithMessage("O número da apólice é obrigatório.");

        RuleFor(x => x.Franquia)
            .GreaterThanOrEqualTo(0).WithMessage("O valor da franquia não pode ser negativo.");

        RuleFor(x => x.ValorFipeReferencia)
            .GreaterThan(0).WithMessage("O valor da Tabela FIPE deve ser superior a zero.");

        RuleFor(x => x.DataOcorrenciaUtc)
            .LessThanOrEqualTo(DateTime.UtcNow.AddMinutes(5)).WithMessage("A data da ocorrência não pode ser futura.");

        RuleFor(x => x.UsuarioCriador)
            .NotEmpty().WithMessage("O usuário criador é obrigatório.");
    }
}

public sealed class AbrirSinistroCommandHandler : IRequestHandler<AbrirSinistroCommand, Result<Guid>>
{
    private readonly ISinistroRepository _sinistroRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMessageBus _messageBus;
    private readonly AbrirSinistroCommandValidator _validator;

    public AbrirSinistroCommandHandler(
        ISinistroRepository sinistroRepository,
        IUnitOfWork unitOfWork,
        IMessageBus messageBus)
    {
        _sinistroRepository = sinistroRepository;
        _unitOfWork = unitOfWork;
        _messageBus = messageBus;
        _validator = new AbrirSinistroCommandValidator();
    }

    public async Task<Result<Guid>> Handle(AbrirSinistroCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return Result<Guid>.Failure(string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)), "VALIDACAO_FALHOU");

        var existente = await _sinistroRepository.ObterPorNumeroAsync(request.NumeroSinistro, cancellationToken);
        if (existente is not null)
            return Result<Guid>.Failure($"Já existe um sinistro registrado com o número '{request.NumeroSinistro}'.", "SINISTRO_DUPLICADO");

        var sinistro = Sinistro.Abrir(
            request.NumeroSinistro,
            new PlacaVeiculo(request.PlacaVeiculo),
            new NumeroApolice(request.NumeroApolice),
            ValorMonetario.De(request.Franquia),
            ValorMonetario.De(request.ValorFipeReferencia),
            request.DataOcorrenciaUtc,
            request.UsuarioCriador,
            request.IpOrigem);

        await _sinistroRepository.AdicionarAsync(sinistro, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        // Publica Evento de Integração no RabbitMQ
        await _messageBus.PublishAsync(
            new SinistroCriadoIntegrationEvent(
                sinistro.Id,
                sinistro.NumeroSinistro,
                sinistro.PlacaVeiculo.Valor,
                sinistro.NumeroApolice.Codigo,
                sinistro.DataOcorrenciaUtc,
                DateTime.UtcNow),
            "sinistro.evento.criado",
            cancellationToken);

        return Result<Guid>.Success(sinistro.Id);
    }
}
