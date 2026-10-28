using FluentValidation;
using MediatR;
using Sinistros.Application.Common;
using Sinistros.Application.Events;
using Sinistros.Domain.DomainServices;
using Sinistros.Domain.Enums;
using Sinistros.Domain.Exceptions;
using Sinistros.Domain.Repositories;

namespace Sinistros.Application.Commands.AprovarSinistro;

public sealed record AprovarSinistroCommand(
    Guid SinistroId,
    string UsuarioAprovador,
    NivelAlcada NivelUsuario,
    string Justificativa,
    string? IpOrigem = null) : IRequest<Result>;

public sealed class AprovarSinistroCommandValidator : AbstractValidator<AprovarSinistroCommand>
{
    public AprovarSinistroCommandValidator()
    {
        RuleFor(x => x.SinistroId).NotEmpty().WithMessage("O identificador do sinistro é obrigatório.");
        RuleFor(x => x.UsuarioAprovador).NotEmpty().WithMessage("O usuário aprovador é obrigatório.");
        RuleFor(x => x.Justificativa).NotEmpty().MinimumLength(10).WithMessage("A justificativa de aprovação deve conter no mínimo 10 caracteres.");
    }
}

public sealed class AprovarSinistroCommandHandler : IRequestHandler<AprovarSinistroCommand, Result>
{
    private readonly ISinistroRepository _sinistroRepository;
    private readonly AlcadaAprovacaoService _alcadaService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMessageBus _messageBus;
    private readonly AprovarSinistroCommandValidator _validator;

    public AprovarSinistroCommandHandler(
        ISinistroRepository sinistroRepository,
        AlcadaAprovacaoService alcadaService,
        IUnitOfWork unitOfWork,
        IMessageBus messageBus)
    {
        _sinistroRepository = sinistroRepository;
        _alcadaService = alcadaService;
        _unitOfWork = unitOfWork;
        _messageBus = messageBus;
        _validator = new AprovarSinistroCommandValidator();
    }

    public async Task<Result> Handle(AprovarSinistroCommand request, CancellationToken cancellationToken)
    {
        var validation = await _validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return Result.Failure(string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)), "VALIDACAO_FALHOU");

        var sinistro = await _sinistroRepository.ObterPorIdAsync(request.SinistroId, cancellationToken);
        if (sinistro is null)
            return Result.Failure($"Sinistro '{request.SinistroId}' não foi localizado.", "SINISTRO_NAO_ENCONTRADO");

        try
        {
            sinistro.Aprovar(request.UsuarioAprovador, request.NivelUsuario, request.Justificativa, _alcadaService, request.IpOrigem);
        }
        catch (AlcadaInsuficienteException ex)
        {
            return Result.Failure(ex.Message, "ALCADA_INSUFICIENTE");
        }
        catch (RegraNegocioException ex)
        {
            return Result.Failure(ex.Message, ex.CodigoRegra);
        }

        await _sinistroRepository.AtualizarAsync(sinistro, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        await _messageBus.PublishAsync(
            new SinistroAprovadoIntegrationEvent(
                sinistro.Id,
                sinistro.NumeroSinistro,
                request.UsuarioAprovador,
                request.NivelUsuario.ToString(),
                sinistro.Parecer?.ValorLiquidoIndenizar.Quantia ?? 0m,
                DateTime.UtcNow),
            "sinistro.evento.aprovado",
            cancellationToken);

        return Result.Success();
    }
}
