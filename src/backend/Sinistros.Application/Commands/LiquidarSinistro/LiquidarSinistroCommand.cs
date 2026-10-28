using FluentValidation;
using MediatR;
using Sinistros.Application.Common;
using Sinistros.Application.Events;
using Sinistros.Domain.Exceptions;
using Sinistros.Domain.Repositories;

namespace Sinistros.Application.Commands.LiquidarSinistro;

public sealed record LiquidarSinistroCommand(
    Guid SinistroId,
    string UsuarioFinanceiro,
    string? IpOrigem = null) : IRequest<Result>;

public sealed class LiquidarSinistroCommandHandler : IRequestHandler<LiquidarSinistroCommand, Result>
{
    private readonly ISinistroRepository _sinistroRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMessageBus _messageBus;

    public LiquidarSinistroCommandHandler(
        ISinistroRepository sinistroRepository,
        IUnitOfWork unitOfWork,
        IMessageBus messageBus)
    {
        _sinistroRepository = sinistroRepository;
        _unitOfWork = unitOfWork;
        _messageBus = messageBus;
    }

    public async Task<Result> Handle(LiquidarSinistroCommand request, CancellationToken cancellationToken)
    {
        var sinistro = await _sinistroRepository.ObterPorIdAsync(request.SinistroId, cancellationToken);
        if (sinistro is null)
            return Result.Failure($"Sinistro '{request.SinistroId}' não foi localizado.", "SINISTRO_NAO_ENCONTRADO");

        try
        {
            sinistro.Liquidar(request.UsuarioFinanceiro, request.IpOrigem);
        }
        catch (RegraNegocioException ex)
        {
            return Result.Failure(ex.Message, ex.CodigoRegra);
        }

        await _sinistroRepository.AtualizarAsync(sinistro, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        await _messageBus.PublishAsync(
            new SinistroLiquidadoIntegrationEvent(
                sinistro.Id,
                sinistro.NumeroSinistro,
                sinistro.Parecer?.ValorLiquidoIndenizar.Quantia ?? 0m,
                DateTime.UtcNow),
            "sinistro.evento.liquidado",
            cancellationToken);

        return Result.Success();
    }
}
