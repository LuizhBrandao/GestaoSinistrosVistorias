using Sinistros.Domain.Aggregates;
using Sinistros.Domain.Enums;
using Sinistros.Domain.Events;
using Sinistros.Domain.Exceptions;
using Sinistros.Domain.ValueObjects;
using Xunit;

namespace Sinistros.Domain.Tests;

public class VistoriaTests
{
    [Fact]
    public void AdicionarAvaria_DeveAcumularCustoTotalCorretamente()
    {
        // Arrange
        var vistoria = Vistoria.Agendar(Guid.NewGuid(), "perito.carlos");

        // Act
        vistoria.AdicionarAvaria("Capô Dianteiro", TipoAcaoAvaria.Substituir, ValorMonetario.De(500m), ValorMonetario.De(1_500m));
        vistoria.AdicionarAvaria("Farol Esquerdo", TipoAcaoAvaria.Substituir, ValorMonetario.De(200m), ValorMonetario.De(800m));

        // Assert: 2000 + 1000 = 3000
        Assert.Equal(3_000m, vistoria.CustoTotalOrcado.Quantia);
        Assert.Equal(2, vistoria.Avarias.Count);
    }

    [Fact]
    public void Concluir_SemEvidenciasFotograficas_DeveLancarExcecaoAntiFraude()
    {
        // Arrange
        var vistoria = Vistoria.Agendar(Guid.NewGuid(), "perito.carlos");
        vistoria.AdicionarAvaria("Para-choque", TipoAcaoAvaria.Recuperar, ValorMonetario.De(300m), ValorMonetario.De(0m));

        // Act & Assert (Vistoria sem fotos não pode ser concluída)
        Assert.Throws<RegraNegocioException>(() => vistoria.Concluir("Laudo preliminar"));
    }

    [Fact]
    public void Concluir_ComEvidenciasEAvarias_DeveConcluirEEmitirEvento()
    {
        // Arrange
        var vistoria = Vistoria.Agendar(Guid.NewGuid(), "perito.carlos");
        vistoria.AdicionarAvaria("Para-choque Dianteiro", TipoAcaoAvaria.Recuperar, ValorMonetario.De(400m), ValorMonetario.De(200m));
        vistoria.RegistrarEvidencia(
            "https://cdn.sinistros.local/orig/foto1.jpg",
            "https://cdn.sinistros.local/comp/foto1.webp",
            "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855",
            new CoordenadasGps(-23.550520, -46.633308),
            DateTime.UtcNow.AddMinutes(-10));

        // Act
        vistoria.Concluir("Avarias condizentes com a dinâmica declarada da colisão.");

        // Assert
        Assert.Equal(StatusVistoria.Concluida, vistoria.Status);
        Assert.NotNull(vistoria.DataConclusaoUtc);
        Assert.Contains(vistoria.DomainEvents, e => e is VistoriaConcluidaDomainEvent);
    }
}
