using Sinistros.Domain.Exceptions;

namespace Sinistros.Domain.ValueObjects;

public sealed record NumeroApolice
{
    public string Codigo { get; }

    public NumeroApolice(string codigo)
    {
        if (string.IsNullOrWhiteSpace(codigo))
            throw new RegraNegocioException("APOLICE_OBRIGATORIA", "O número da apólice é obrigatório.");

        var limpo = codigo.Trim().ToUpperInvariant();
        if (limpo.Length < 6 || limpo.Length > 30)
            throw new RegraNegocioException("APOLICE_INVALIDA", "O número da apólice deve conter entre 6 e 30 caracteres alfanuméricos.");

        Codigo = limpo;
    }

    public override string ToString() => Codigo;

    public static implicit operator string(NumeroApolice apolice) => apolice.Codigo;
    public static explicit operator NumeroApolice(string codigo) => new(codigo);
}
