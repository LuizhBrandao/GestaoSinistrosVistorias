using Sinistros.Domain.Exceptions;

namespace Sinistros.Domain.ValueObjects;

/// <summary>
/// Objeto de Valor imutável para quantias monetárias com precisão de 2 casas decimais.
/// </summary>
public readonly record struct ValorMonetario : IComparable<ValorMonetario>
{
    public decimal Quantia { get; }
    public string Moeda { get; }

    public static ValorMonetario Zero => new(0m);

    public ValorMonetario(decimal quantia, string moeda = "BRL")
    {
        if (quantia < 0)
            throw new RegraNegocioException("VALOR_NEGATIVO", "A quantia monetária não pode ser negativa.");

        Quantia = Math.Round(quantia, 2, MidpointRounding.AwayFromZero);
        Moeda = string.IsNullOrWhiteSpace(moeda) ? "BRL" : moeda.ToUpperInvariant();
    }

    public static ValorMonetario De(decimal quantia) => new(quantia);

    public ValorMonetario Somar(ValorMonetario outro)
    {
        ValidarMoeda(outro);
        return new(Quantia + outro.Quantia, Moeda);
    }

    public ValorMonetario Subtrair(ValorMonetario outro)
    {
        ValidarMoeda(outro);
        var resultado = Quantia - outro.Quantia;
        return new(Math.Max(0m, resultado), Moeda);
    }

    public ValorMonetario Multiplicar(decimal fator)
    {
        if (fator < 0)
            throw new RegraNegocioException("FATOR_NEGATIVO", "O fator multiplicador não pode ser negativo.");

        return new(Quantia * fator, Moeda);
    }

    private void ValidarMoeda(ValorMonetario outro)
    {
        if (Moeda != outro.Moeda)
            throw new RegraNegocioException("MOEDA_DIVERGENTE", $"Não é permitido operar entre moedas distintas: {Moeda} e {outro.Moeda}.");
    }

    public int CompareTo(ValorMonetario other)
    {
        ValidarMoeda(other);
        return Quantia.CompareTo(other.Quantia);
    }

    public static bool operator >(ValorMonetario left, ValorMonetario right) => left.CompareTo(right) > 0;
    public static bool operator <(ValorMonetario left, ValorMonetario right) => left.CompareTo(right) < 0;
    public static bool operator >=(ValorMonetario left, ValorMonetario right) => left.CompareTo(right) >= 0;
    public static bool operator <=(ValorMonetario left, ValorMonetario right) => left.CompareTo(right) <= 0;

    public static ValorMonetario operator +(ValorMonetario left, ValorMonetario right) => left.Somar(right);
    public static ValorMonetario operator -(ValorMonetario left, ValorMonetario right) => left.Subtrair(right);

    public override string ToString() => $"{Moeda} {Quantia:N2}";
}
