using Sinistros.Domain.Entities;
using Sinistros.Domain.Enums;
using Sinistros.Domain.Exceptions;
using Sinistros.Domain.ValueObjects;

namespace Sinistros.Domain.DomainServices;

/// <summary>
/// Motor de Regras de Domínio para cálculo de indenização securitária (SUSEP - Perda Parcial vs. Perda Total).
/// </summary>
public sealed class MotorCalculoIndenizacaoService
{
    private const decimal PercentualLimitePerdaTotal = 0.75m; // 75% da FIPE
    private readonly AlcadaAprovacaoService _alcadaService;

    public MotorCalculoIndenizacaoService(AlcadaAprovacaoService alcadaService)
    {
        _alcadaService = alcadaService ?? throw new ArgumentNullException(nameof(alcadaService));
    }

    public ParecerRegulacao CalcularParecer(
        Guid sinistroId,
        ValorMonetario custoTotalOrcado,
        ValorMonetario valorFipeReferencia,
        ValorMonetario franquia,
        string? observacoes = null)
    {
        if (valorFipeReferencia.Quantia <= 0)
            throw new RegraNegocioException("FIPE_INVALIDA", "O valor de referência da Tabela FIPE deve ser superior a zero.");

        var limitePerdaTotal = valorFipeReferencia.Multiplicar(PercentualLimitePerdaTotal);
        TipoPerda tipoPerda;
        ValorMonetario valorLiquido;

        if (custoTotalOrcado >= limitePerdaTotal)
        {
            // Indenização Integral / Perda Total: 100% da FIPE, sem desconto de franquia
            tipoPerda = TipoPerda.PerdaTotal;
            valorLiquido = valorFipeReferencia;
        }
        else
        {
            // Perda Parcial: Custo orçado abatido o valor da franquia
            tipoPerda = TipoPerda.PerdaParcial;
            valorLiquido = custoTotalOrcado.Subtrair(franquia);
        }

        var alcadaRequerida = _alcadaService.DeterminarAlcada(valorLiquido, tipoPerda);

        return new ParecerRegulacao(
            sinistroId,
            tipoPerda,
            custoTotalOrcado,
            valorFipeReferencia,
            franquia,
            valorLiquido,
            alcadaRequerida,
            observacoes);
    }
}
