using FluentValidation;
using MediatR;
using Sinistros.Application.Common;
using Sinistros.Application.Events;
using Sinistros.Domain.DomainServices;
using Sinistros.Domain.Enums;
using Sinistros.Domain.Repositories;
using Sinistros.Domain.ValueObjects;

namespace Sinistros.Application.Commands.ConcluirVistoria;

public sealed record ItemAvariaInput(
    string Peca,
    TipoAcaoAvaria TipoAcao,
    decimal CustoMaoDeObra,
    decimal CustoPecas);

public sealed record FotoEvidenciaInput(
    string UrlOriginal,
    string UrlComprimida,
    string HashArquivoSha256,
    double? Latitude = null,
    double? Longitude = null,
    DateTime? DataCapturaExif = null);

public sealed record ConcluirVistoriaCommand(
    Guid VistoriaId,
    string ObservacoesPerito,
    IReadOnlyList<ItemAvariaInput> Avarias,
    IReadOnlyList<FotoEvidenciaInput> Evidencias,
    string UsuarioExecutor,
    string? IpOrigem = null) : IRequest<Result>;

public sealed class ConcluirVistoriaCommandValidator : AbstractValidator<ConcluirVistoriaCommand>
{
    public ConcluirVistoriaCommandValidator()
    {
        RuleFor(x => x.VistoriaId).NotEmpty().WithMessage("O identificador da vistoria é obrigatório.");
        RuleFor(x => x.ObservacoesPerito).NotEmpty().WithMessage("O laudo/observação técnica do perito é obrigatório.");
        RuleFor(x => x.Evidencias).NotEmpty().WithMessage("Ao menos uma evidência fotográfica deve ser anexada.");
        RuleFor(x => x.UsuarioExecutor).NotEmpty().WithMessage("O usuário executor é obrigatório.");
    }
}

public sealed class ConcluirVistoriaCommandHandler : IRequestHandler<ConcluirVistoriaCommand, Result>
{
    private readonly IVistoriaRepository _vistoriaRepository;
    private readonly ISinistroRepository _sinistroRepository;
    private readonly MotorCalculoIndenizacaoService _motorCalculo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMessageBus _messageBus;
    private readonly ConcluirVistoriaCommandValidator _validator;

    public ConcluirVistoriaCommandHandler(
        IVistoriaRepository vistoriaRepository,
        ISinistroRepository sinistroRepository,
        MotorCalculoIndenizacaoService motorCalculo,
        IUnitOfWork unitOfWork,
        IMessageBus messageBus)
    {
        _vistoriaRepository = vistoriaRepository;
        _sinistroRepository = sinistroRepository;
        _motorCalculo = motorCalculo;
        _unitOfWork = unitOfWork;
        _messageBus = messageBus;
        _validator = new ConcluirVistoriaCommandValidator();
    }

    public async Task<Result> Handle(ConcluirVistoriaCommand request, CancellationToken cancellationToken)
    {
        var validation = await _validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return Result.Failure(string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)), "VALIDACAO_FALHOU");

        var vistoria = await _vistoriaRepository.ObterPorIdAsync(request.VistoriaId, cancellationToken);
        if (vistoria is null)
            return Result.Failure($"Vistoria '{request.VistoriaId}' não encontrada.", "VISTORIA_NAO_ENCONTRADA");

        var sinistro = await _sinistroRepository.ObterPorIdAsync(vistoria.SinistroId, cancellationToken);
        if (sinistro is null)
            return Result.Failure($"Sinistro associado '{vistoria.SinistroId}' não encontrado.", "SINISTRO_NAO_ENCONTRADO");

        // Adiciona itens inspecionados
        foreach (var avaria in request.Avarias)
        {
            vistoria.AdicionarAvaria(
                avaria.Peca,
                avaria.TipoAcao,
                ValorMonetario.De(avaria.CustoMaoDeObra),
                ValorMonetario.De(avaria.CustoPecas));
        }

        // Adiciona fotos
        foreach (var foto in request.Evidencias)
        {
            CoordenadasGps? gps = foto.Latitude.HasValue && foto.Longitude.HasValue
                ? new CoordenadasGps(foto.Latitude.Value, foto.Longitude.Value)
                : null;

            vistoria.RegistrarEvidencia(
                foto.UrlOriginal,
                foto.UrlComprimida,
                foto.HashArquivoSha256,
                gps,
                foto.DataCapturaExif);
        }

        vistoria.Concluir(request.ObservacoesPerito);

        // Processa regulação e cálculo de indenização
        sinistro.ProcessarLaudoVistoria(
            vistoria.CustoTotalOrcado,
            _motorCalculo,
            request.UsuarioExecutor,
            request.ObservacoesPerito,
            request.IpOrigem);

        await _vistoriaRepository.AtualizarAsync(vistoria, cancellationToken);
        await _sinistroRepository.AtualizarAsync(sinistro, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        await _messageBus.PublishAsync(
            new VistoriaConcluidaIntegrationEvent(
                vistoria.Id,
                sinistro.Id,
                vistoria.PeritoId,
                vistoria.CustoTotalOrcado.Quantia,
                vistoria.Avarias.Count,
                vistoria.Evidencias.Count,
                DateTime.UtcNow),
            "vistoria.evento.concluida",
            cancellationToken);

        return Result.Success();
    }
}
