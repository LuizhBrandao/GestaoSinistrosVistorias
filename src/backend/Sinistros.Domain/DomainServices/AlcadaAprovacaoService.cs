using Sinistros.Domain.Enums;
using Sinistros.Domain.ValueObjects;

namespace Sinistros.Domain.DomainServices;

/// <summary>
/// Serviço de Domínio para cálculo de limites de alçadas corporativas e governança de aprovação.
/// </summary>
public sealed class AlcadaAprovacaoService
{
    public const decimal LimiteOperacional = 10_000.00m;
    public const decimal LimiteSupervisao = 50_000.00m;

    public NivelAlcada DeterminarAlcada(ValorMonetario valor, TipoPerda tipoPerda)
    {
        // Regra de Governança: Perda Total requer no mínimo alçada Gerencial independente do valor
        if (tipoPerda == TipoPerda.PerdaTotal)
            return NivelAlcada.Gerencial;

        if (valor.Quantia <= LimiteOperacional)
            return NivelAlcada.Operacional;

        if (valor.Quantia <= LimiteSupervisao)
            return NivelAlcada.Supervisao;

        return NivelAlcada.Gerencial;
    }

    public bool PossuiAlcadaSuficiente(NivelAlcada nivelUsuario, NivelAlcada nivelRequerido)
    {
        return nivelUsuario >= nivelRequerido;
    }
}
