using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sinistros.Domain.Aggregates;
using Sinistros.Domain.ValueObjects;

namespace Sinistros.Infrastructure.Persistence.Configurations;

public sealed class SinistroConfiguration : IEntityTypeConfiguration<Sinistro>
{
    public void Configure(EntityTypeBuilder<Sinistro> builder)
    {
        builder.ToTable("sinistros", "sinistros");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.NumeroSinistro)
            .HasMaxLength(30)
            .IsRequired();

        builder.HasIndex(x => x.NumeroSinistro)
            .IsUnique();

        builder.Property(x => x.PlacaVeiculo)
            .HasConversion(p => p.Valor, v => new PlacaVeiculo(v))
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(x => x.NumeroApolice)
            .HasConversion(a => a.Codigo, v => new NumeroApolice(v))
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.Franquia)
            .HasConversion(f => f.Quantia, v => ValorMonetario.De(v))
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.ValorFipeReferencia)
            .HasConversion(f => f.Quantia, v => ValorMonetario.De(v))
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.DataOcorrenciaUtc)
            .IsRequired();

        builder.Property(x => x.DataAberturaUtc)
            .IsRequired();

        // Relacionamento 1:1 com ParecerRegulacao
        builder.HasOne(x => x.Parecer)
            .WithOne()
            .HasForeignKey<Sinistros.Domain.Entities.ParecerRegulacao>(p => p.SinistroId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relacionamento 1:N com RegistroAuditoria
        builder.HasMany(x => x.Auditoria)
            .WithOne()
            .HasForeignKey(a => a.SinistroId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configura Backing Fields para coleções DDD encapsuladas
        builder.Navigation(x => x.Auditoria)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Ignore(x => x.DomainEvents);
    }
}
