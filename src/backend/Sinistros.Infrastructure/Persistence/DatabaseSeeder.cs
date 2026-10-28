using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sinistros.Domain.Aggregates;
using Sinistros.Domain.DomainServices;
using Sinistros.Domain.Enums;
using Sinistros.Domain.ValueObjects;

namespace Sinistros.Infrastructure.Persistence;

public sealed class DatabaseSeeder
{
    private readonly SinistrosDbContext _context;
    private readonly MotorCalculoIndenizacaoService _motorCalculo;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(
        SinistrosDbContext context,
        MotorCalculoIndenizacaoService motorCalculo,
        ILogger<DatabaseSeeder> logger)
    {
        _context = context;
        _motorCalculo = motorCalculo;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await _context.Database.EnsureCreatedAsync(cancellationToken);

        if (await _context.Sinistros.AnyAsync(cancellationToken))
        {
            _logger.LogInformation("Banco de dados já contém registros. Seed ignorado.");
            return;
        }

        _logger.LogInformation("Iniciando carga de dados de demonstração (Seed Data)...");

        // 1. Sinistro Aberto aguardando Vistoria (Toyota Corolla)
        var s1 = Sinistro.Abrir(
            "SIN-2026-0001",
            new PlacaVeiculo("BRA2E19"),
            new NumeroApolice("APOL-BR-88231"),
            ValorMonetario.De(2_500m),
            ValorMonetario.De(125_000m),
            DateTime.UtcNow.AddDays(-2),
            "portal.segurado");

        s1.SolicitarVistoria("atendimento.central");

        var v1 = Vistoria.Agendar(s1.Id, "perito.carlos@vistoria.local");

        // 2. Sinistro com Vistoria Concluída e Perda Parcial aguardando aprovação de Supervisão (Honda Civic)
        var s2 = Sinistro.Abrir(
            "SIN-2026-0002",
            new PlacaVeiculo("ABC1D23"),
            new NumeroApolice("APOL-BR-77112"),
            ValorMonetario.De(3_000m),
            ValorMonetario.De(110_000m),
            DateTime.UtcNow.AddDays(-5),
            "corretora.porto");

        s2.SolicitarVistoria("sistema.triagem");

        var v2 = Vistoria.Agendar(s2.Id, "perita.ana@vistoria.local");
        v2.AdicionarAvaria("Capô Dianteiro", TipoAcaoAvaria.Substituir, ValorMonetario.De(800m), ValorMonetario.De(3_200m));
        v2.AdicionarAvaria("Grade Radiador", TipoAcaoAvaria.Substituir, ValorMonetario.De(300m), ValorMonetario.De(1_500m));
        v2.AdicionarAvaria("Para-choque Dianteiro", TipoAcaoAvaria.Recuperar, ValorMonetario.De(1_200m), ValorMonetario.De(800m));
        v2.AdicionarAvaria("Farol LED Direito", TipoAcaoAvaria.Substituir, ValorMonetario.De(400m), ValorMonetario.De(4_800m));
        v2.RegistrarEvidencia(
            "https://cdn.sinistros.local/orig/civic_frontal.jpg",
            "https://cdn.sinistros.local/comp/civic_frontal.webp",
            "a1b2c3d4e5f67890123456789abcdef0123456789abcdef0123456789abcdef0",
            new CoordenadasGps(-23.550520, -46.633308),
            DateTime.UtcNow.AddDays(-4));

        v2.Concluir("Danos na dianteira direita compatíveis com colisão contra obstáculo fixo em baixa velocidade.");
        s2.ProcessarLaudoVistoria(v2.CustoTotalOrcado, _motorCalculo, "sistema.integracao");

        // 3. Sinistro com Perda Total aguardando aprovação Gerencial (Jeep Compass)
        var s3 = Sinistro.Abrir(
            "SIN-2026-0003",
            new PlacaVeiculo("KXZ9F44"),
            new NumeroApolice("APOL-BR-99443"),
            ValorMonetario.De(4_500m),
            ValorMonetario.De(140_000m),
            DateTime.UtcNow.AddDays(-7),
            "portal.segurado");

        s3.SolicitarVistoria("sistema.triagem");

        var v3 = Vistoria.Agendar(s3.Id, "perito.carlos@vistoria.local");
        v3.AdicionarAvaria("Monobloco e Longarinas", TipoAcaoAvaria.Substituir, ValorMonetario.De(15_000m), ValorMonetario.De(35_000m));
        v3.AdicionarAvaria("Motor e Câmbio", TipoAcaoAvaria.Substituir, ValorMonetario.De(12_000m), ValorMonetario.De(40_000m));
        v3.AdicionarAvaria("Bolsas de Airbag e Painel", TipoAcaoAvaria.Substituir, ValorMonetario.De(5_000m), ValorMonetario.De(15_000m));
        v3.RegistrarEvidencia(
            "https://cdn.sinistros.local/orig/compass_capotamento.jpg",
            "https://cdn.sinistros.local/comp/compass_capotamento.webp",
            "b2c3d4e5f6a17890123456789abcdef0123456789abcdef0123456789abcdef0",
            new CoordenadasGps(-23.561684, -46.655981),
            DateTime.UtcNow.AddDays(-6));

        v3.Concluir("Danos estruturais severos na coluna A, teto e suspensão. Inviabilidade econômica de recuperação (Perda Total).");
        s3.ProcessarLaudoVistoria(v3.CustoTotalOrcado, _motorCalculo, "sistema.integracao");

        await _context.Sinistros.AddRangeAsync([s1, s2, s3], cancellationToken);
        await _context.Vistorias.AddRangeAsync([v1, v2, v3], cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Seed Data concluído com sucesso: 3 sinistros e 3 vistorias criadas.");
    }
}
