using System.Text.RegularExpressions;
using Sinistros.Domain.Exceptions;

namespace Sinistros.Domain.ValueObjects;

/// <summary>
/// Objeto de Valor imutável para placa de veículo automotor (Padrão Mercosul e Tradicional Brasileiro).
/// </summary>
public sealed partial class PlacaVeiculo : IEquatable<PlacaVeiculo>
{
    private static readonly Regex PlacaRegex = MyRegex();

    public string Valor { get; }

    public PlacaVeiculo(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            throw new RegraNegocioException("PLACA_OBRIGATORIA", "A placa do veículo deve ser informada.");

        var limpa = valor.Trim().ToUpperInvariant().Replace("-", "");

        if (!PlacaRegex.IsMatch(limpa))
            throw new RegraNegocioException("PLACA_INVALIDA", $"O formato da placa '{valor}' é inválido. Padrões aceitos: ABC1234 ou ABC1D23.");

        Valor = limpa;
    }

    public string Formatada => Valor.Length == 7 ? $"{Valor[..3]}-{Valor[3..]}" : Valor;

    public bool Equals(PlacaVeiculo? other) => other is not null && Valor == other.Valor;
    public override bool Equals(object? obj) => obj is PlacaVeiculo other && Equals(other);
    public override int GetHashCode() => Valor.GetHashCode();
    public override string ToString() => Formatada;

    public static implicit operator string(PlacaVeiculo placa) => placa.Valor;
    public static explicit operator PlacaVeiculo(string valor) => new(valor);

    [GeneratedRegex(@"^[A-Z]{3}[0-9][A-Z0-9][0-9]{2}$", RegexOptions.Compiled)]
    private static partial Regex MyRegex();
}
