using MediatR;
using Microsoft.AspNetCore.Mvc;
using Sinistros.Application.Commands.AprovarSinistro;
using Sinistros.Application.Commands.LiquidarSinistro;
using Sinistros.Application.Queries;
using Sinistros.Domain.Enums;

namespace Sinistros.Web.Controllers;

public class SinistrosController : Controller
{
    private readonly IMediator _mediator;
    private readonly ILogger<SinistrosController> _logger;

    public SinistrosController(IMediator _mediator, ILogger<SinistrosController> logger)
    {
        this._mediator = _mediator;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var result = await _mediator.Send(new ObterSinistrosPendentesQuery());
        if (result.IsFailure)
        {
            ViewBag.ErrorMessage = result.Error;
            return View(Array.Empty<Sinistros.Application.DTOs.SinistroResumoDto>());
        }

        return View(result.Value);
    }

    [HttpGet]
    public async Task<IActionResult> Detalhes(Guid id)
    {
        var result = await _mediator.Send(new ObterSinistroPorIdQuery(id));
        if (result.IsFailure)
        {
            TempData["ErrorMessage"] = result.Error;
            return RedirectToAction(nameof(Index));
        }

        var vistoriaResult = await _mediator.Send(new ObterVistoriaPorSinistroIdQuery(id));
        ViewBag.Vistoria = vistoriaResult.IsSuccess ? vistoriaResult.Value : null;

        return View(result.Value);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Aprovar(Guid id, NivelAlcada nivelUsuario, string justificativa)
    {
        var command = new AprovarSinistroCommand(
            id,
            User.Identity?.Name ?? "gestor.regulação",
            nivelUsuario,
            justificativa,
            HttpContext.Connection.RemoteIpAddress?.ToString());

        var result = await _mediator.Send(command);
        if (result.IsFailure)
        {
            TempData["ErrorMessage"] = $"Falha na aprovação: {result.Error}";
        }
        else
        {
            TempData["SuccessMessage"] = "Sinistro aprovado com sucesso! Trilha de auditoria registrada.";
        }

        return RedirectToAction(nameof(Detalhes), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Liquidar(Guid id)
    {
        var command = new LiquidarSinistroCommand(
            id,
            User.Identity?.Name ?? "financeiro.pagamentos",
            HttpContext.Connection.RemoteIpAddress?.ToString());

        var result = await _mediator.Send(command);
        if (result.IsFailure)
        {
            TempData["ErrorMessage"] = $"Falha na liquidação: {result.Error}";
        }
        else
        {
            TempData["SuccessMessage"] = "Sinistro liquidado financeiramente com sucesso!";
        }

        return RedirectToAction(nameof(Detalhes), new { id });
    }
}
