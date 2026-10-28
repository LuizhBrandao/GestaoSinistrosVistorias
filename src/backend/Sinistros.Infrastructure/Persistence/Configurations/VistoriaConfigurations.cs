using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sinistros.Domain.Aggregates;
using Sinistros.Domain.Entities;
using Sinistros.Domain.ValueObjects;

namespace Sinistros.Infrastructure.Persistence.Configurations;

public sealed class VistoriaConfiguration : IEntityTypeConfiguration<Vistoria>
{
    public void Configure(EntityTypeBuilder<Vistoria> builder)
    {
        builder.ToTable("vistorias", "vistorias");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.PeritoId)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.DataAgendamentoUtc)
            .IsRequired();

        builder.Property(x => x.ObservacoesPerito)
            .HasMaxLength(2000);

        builder.HasIndex(x => x.SinistroId);

        builder.HasMany(x => x.Avarias)
            .WithOne()
            .HasForeignKey(a => a.VistoriaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Evidencias)
            .WithOne()
            .HasForeignKey(e => e.VistoriaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Avarias)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(x => x.Evidencias)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Ignore(x => x.DomainEvents);
        builder.Ignore(x => x.CustoTotalOrcado);
    }
}

public sealed class ItemAvariaConfiguration : IEntityTypeConfiguration<ItemAvaria>
{
    public void Configure(EntityTypeBuilder<ItemAvaria> builder)
    {
        builder.ToTable("itens_avaria", "vistorias");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Peca)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.TipoAcao)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.CustoMaoDeObra)
            .HasConversion(v => v.Quantia, q => ValorMonetario.De(q))
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.CustoPecas)
            .HasConversion(v => v.Quantia, q => ValorMonetario.De(q))
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Ignore(x => x.CustoTotal);
    }
}

public sealed class FotoEvidenciaConfiguration : IEntityTypeConfiguration<FotoEvidencia>
{
    public void Configure(EntityTypeBuilder<FotoEvidencia> builder)
    {
        builder.ToTable("fotos_evidencia", "vistorias");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UrlOriginal)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.UrlComprimida)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.HashArquivoSha256)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(x => x.DataUploadUtc)
            .IsRequired();

        // Mapeamento de CoordenadasGps com conversão de valor
        builder.Property(x => x.CoordenadasGps)
            .HasConversion(
                gps => gps.HasValue ? $"{gps.Value.Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture)};{gps.Value.Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}" : null,
                str => ParseGps(str))
            .HasMaxLength(50);
    }

    private static CoordenadasGps? ParseGps(string? str)
    {
        if (string.IsNullOrWhiteSpace(str)) return null;
        var parts = str.Split(';');
        if (parts.Length == 2 &&
            double.TryParse(parts[0], System.Globalization.CultureInfo.InvariantCulture, out var lat) &&
            double.TryParse(parts[1], System.Globalization.CultureInfo.InvariantCulture, out var lon))
        {
            return new CoordenadasGps(lat, lon);
        }
        return null;
    }
}
