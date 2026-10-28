using FluentValidation;
using MediatR;
using Sinistros.Application.Common;
using Sinistros.Application.Events;
using Sinistros.Domain.Aggregates;
using Sinistros.Domain.Repositories;

namespace Sinistros.Application.Commands.SolicitarVistoria;

public sealed record SolicitarVistoriaCommand(
    Guid SinistroId,
    string PeritoId,
    string UsuarioSolicitante,
    string? IpOrigem = null) : IRequest<Result<Guid>>;

public sealed class SolicitarVistoriaCommandValidator : AbstractValidator<SolicitarVistoriaCommand>
{
    public SolicitarVistoriaCommandValidator()
    {
        RuleFor(x => x.SinistroId).NotEmpty().WithMessage("O identificador do sinistro é obrigatório.");
        RuleFor(x => x.PeritoId).NotEmpty().WithMessage("O perito responsável pela vistoria deve ser informado.");
        RuleFor(x => x.UsuarioSolicitante).NotEmpty().WithMessage("O usuário solicitante é obrigatório.");
    }
}

public sealed class SolicitarVistoriaCommandHandler : IRequestHandler<SolicitarVistoriaCommand, Result<Guid>>
{
    private readonly ISinistroRepository _sinistroRepository;
    private readonly IVistoriaRepository _vistoriaRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMessageBus _messageBus;
    private readonly SolicitarVistoriaCommandValidator _validator;

    public SolicitarVistoriaCommandHandler(
        ISinistroRepository sinistroRepository,
        IVistoriaRepository vistoriaRepository,
        IUnitOfWork unitOfWork,
        IMessageBus messageBus)
    {
        _sinistroRepository = sinistroRepository;
        _vistoriaRepository = vistoriaRepository;
        _unitOfWork = unitOfWork;
        _messageBus = messageBus;
        _validator = new SolicitarVistoriaCommandValidator();
    }

    public async Task<Result<Guid>> Handle(SolicitarVistoriaCommand request, CancellationToken cancellationToken)
    {
        var validation = await _validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return Result<Guid>.Failure(string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)), "VALIDACAO_FALHOU");

        var sinistro = await _sinistroRepository.ObterPorIdAsync(request.SinistroId, cancellationToken);
        if (sinistro is null)
            return Result<Guid>.Failure($"Sinistro com ID '{request.SinistroId}' não foi localizado.", "SINISTRO_NAO_ENCONTRADO");

        sinistro.SolicitarVistoria(request.UsuarioSolicitante, request.IpOrigem);

        var vistoria = Vistoria.Agendar(sinistro.Id, request.PeritoId);
        await _vistoriaRepository.AdicionarAsync(vistoria, cancellationToken);
        await _sinistroRepository.AtualizarAsync(sinistro, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        await _messageBus.PublishAsync(
            new VistoriaSolicitadaIntegrationEvent(
                sinistro.Id,
                sinistro.NumeroSinistro,
                sinistro.PlacaVeiculo.Valor,
                DateTime.UtcNow),
            "vistoria.evento.solicitada",
            cancellationToken);

        return Result<Guid>.Success(vistoria.Id);
    }
}
