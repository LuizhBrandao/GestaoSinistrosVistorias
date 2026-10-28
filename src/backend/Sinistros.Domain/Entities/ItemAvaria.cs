using Sinistros.Domain.Common;
using Sinistros.Domain.Enums;
using Sinistros.Domain.Exceptions;
using Sinistros.Domain.ValueObjects;

namespace Sinistros.Domain.Entities;

public sealed class ItemAvaria : Entity<Guid>
{
    public Guid VistoriaId { get; private set; }
    public string Peca { get; private set; } = null!;
    public TipoAcaoAvaria TipoAcao { get; private set; }
    public ValorMonetario CustoMaoDeObra { get; private set; }
    public ValorMonetario CustoPecas { get; private set; }
    public ValorMonetario CustoTotal => CustoMaoDeObra.Somar(CustoPecas);

    private ItemAvaria() : base() { }

    public ItemAvaria(Guid vistoriaId, string peca, TipoAcaoAvaria tipoAcao, ValorMonetario custoMaoDeObra, ValorMonetario custoPecas)
        : base(Guid.NewGuid())
    {
        if (string.IsNullOrWhiteSpace(peca))
            throw new RegraNegocioException("PECA_OBRIGATORIA", "A descrição da peça avariada é obrigatória.");

        VistoriaId = vistoriaId;
        Peca = peca.Trim();
        TipoAcao = tipoAcao;
        CustoMaoDeObra = custoMaoDeObra;
        CustoPecas = custoPecas;
    }
}
