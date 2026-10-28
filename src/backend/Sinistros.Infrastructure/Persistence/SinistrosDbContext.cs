using Microsoft.EntityFrameworkCore;
using Sinistros.Domain.Aggregates;
using Sinistros.Domain.Entities;

namespace Sinistros.Infrastructure.Persistence;

public sealed class SinistrosDbContext : DbContext
{
    public DbSet<Sinistro> Sinistros => Set<Sinistro>();
    public DbSet<Vistoria> Vistorias => Set<Vistoria>();
    public DbSet<ParecerRegulacao> Pareceres => Set<ParecerRegulacao>();
    public DbSet<RegistroAuditoria> RegistrosAuditoria => Set<RegistroAuditoria>();
    public DbSet<ItemAvaria> ItensAvaria => Set<ItemAvaria>();
    public DbSet<FotoEvidencia> FotosEvidencia => Set<FotoEvidencia>();

    public SinistrosDbContext(DbContextOptions<SinistrosDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SinistrosDbContext).Assembly);
    }
}
