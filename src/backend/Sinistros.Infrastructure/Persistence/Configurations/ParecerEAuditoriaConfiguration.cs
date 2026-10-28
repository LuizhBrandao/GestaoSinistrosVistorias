using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sinistros.Domain.Entities;
using Sinistros.Domain.ValueObjects;

namespace Sinistros.Infrastructure.Persistence.Configurations;

public sealed class ParecerRegulacaoConfiguration : IEntityTypeConfiguration<ParecerRegulacao>
{
    public void Configure(EntityTypeBuilder<ParecerRegulacao> builder)
    {
        builder.ToTable("pareceres", "sinistros");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TipoPerda)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.CustoTotalOrcado)
            .HasConversion(v => v.Quantia, q => ValorMonetario.De(q))
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.ValorFipeReferencia)
            .HasConversion(v => v.Quantia, q => ValorMonetario.De(q))
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.FranquiaAplicada)
            .HasConversion(v => v.Quantia, q => ValorMonetario.De(q))
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.ValorLiquidoIndenizar)
            .HasConversion(v => v.Quantia, q => ValorMonetario.De(q))
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.NivelAlcadaRequerido)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.DataCalculoUtc)
            .IsRequired();

        builder.Property(x => x.Observacoes)
            .HasMaxLength(1000);
    }
}

public sealed class RegistroAuditoriaConfiguration : IEntityTypeConfiguration<RegistroAuditoria>
{
    public void Configure(EntityTypeBuilder<RegistroAuditoria> builder)
    {
        builder.ToTable("registros_auditoria", "auditoria");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Usuario)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Acao)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Detalhes)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(x => x.IpOrigem)
            .HasMaxLength(45);

        builder.Property(x => x.DataHoraUtc)
            .IsRequired();

        builder.HasIndex(x => x.SinistroId);
    }
}
