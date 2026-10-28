using Sinistros.Domain.Exceptions;
using Sinistros.Domain.ValueObjects;
using Xunit;

namespace Sinistros.Domain.Tests;

public class ValueObjectsTests
{
    [Theory]
    [InlineData("ABC1234", "ABC-1234")]
    [InlineData("abc-1234", "ABC-1234")]
    [InlineData("bra2e19", "BRA-2E19")]
    [InlineData("BRA2E19", "BRA-2E19")]
    public void PlacaVeiculo_FormatosValidos_DeveNormalizarComSucesso(string input, string esperado)
    {
        var placa = new PlacaVeiculo(input);
        Assert.Equal(esperado, placa.Formatada);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("1234567")]
    [InlineData("ABCD-123")]
    [InlineData("AB12345")]
    public void PlacaVeiculo_FormatosInvalidos_DeveLancarExcecao(string input)
    {
        Assert.Throws<RegraNegocioException>(() => new PlacaVeiculo(input));
    }

    [Fact]
    public void ValorMonetario_OperacoesBasicas_DevemManterImutabilidadeEArredondamento()
    {
        var v1 = ValorMonetario.De(100.505m); // Arredonda para 100.51
        var v2 = ValorMonetario.De(50.25m);

        var soma = v1 + v2;
        var subtracao = v1 - v2;

        Assert.Equal(100.51m, v1.Quantia);
        Assert.Equal(150.76m, soma.Quantia);
        Assert.Equal(50.26m, subtracao.Quantia);
    }

    [Fact]
    public void ValorMonetario_ValorNegativo_DeveLancarExcecao()
    {
        Assert.Throws<RegraNegocioException>(() => ValorMonetario.De(-10m));
    }
}
