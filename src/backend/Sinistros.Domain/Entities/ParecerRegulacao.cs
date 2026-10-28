using Sinistros.Domain.Common;
using Sinistros.Domain.Enums;
using Sinistros.Domain.ValueObjects;

namespace Sinistros.Domain.Entities;

public sealed class ParecerRegulacao : Entity<Guid>
{
    public Guid SinistroId { get; private set; }
    public TipoPerda TipoPerda { get; private set; }
    public ValorMonetario CustoTotalOrcado { get; private set; }
    public ValorMonetario ValorFipeReferencia { get; private set; }
    public ValorMonetario FranquiaAplicada { get; private set; }
    public ValorMonetario ValorLiquidoIndenizar { get; private set; }
    public NivelAlcada NivelAlcadaRequerido { get; private set; }
    public DateTime DataCalculoUtc { get; private set; }
    public string? Observacoes { get; private set; }

    private ParecerRegulacao() : base() { }

    public ParecerRegulacao(
        Guid sinistroId,
        TipoPerda tipoPerda,
        ValorMonetario custoTotalOrcado,
        ValorMonetario valorFipeReferencia,
        ValorMonetario franquiaAplicada,
        ValorMonetario valorLiquidoIndenizar,
        NivelAlcada nivelAlcadaRequerido,
        string? observacoes = null)
        : base(Guid.NewGuid())
    {
        SinistroId = sinistroId;
        TipoPerda = tipoPerda;
        CustoTotalOrcado = custoTotalOrcado;
        ValorFipeReferencia = valorFipeReferencia;
        FranquiaAplicada = franquiaAplicada;
        ValorLiquidoIndenizar = valorLiquidoIndenizar;
        NivelAlcadaRequerido = nivelAlcadaRequerido;
        DataCalculoUtc = DateTime.UtcNow;
        Observacoes = observacoes?.Trim();
    }
}
