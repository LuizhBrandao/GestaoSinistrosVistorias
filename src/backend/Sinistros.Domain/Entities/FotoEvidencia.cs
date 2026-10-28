using Sinistros.Domain.Common;
using Sinistros.Domain.Exceptions;
using Sinistros.Domain.ValueObjects;

namespace Sinistros.Domain.Entities;

public sealed class FotoEvidencia : Entity<Guid>
{
    public Guid VistoriaId { get; private set; }
    public string UrlOriginal { get; private set; } = null!;
    public string UrlComprimida { get; private set; } = null!;
    public string HashArquivoSha256 { get; private set; } = null!;
    public CoordenadasGps? CoordenadasGps { get; private set; }
    public DateTime? DataCapturaExif { get; private set; }
    public DateTime DataUploadUtc { get; private set; }

    private FotoEvidencia() : base() { }

    public FotoEvidencia(
        Guid vistoriaId,
        string urlOriginal,
        string urlComprimida,
        string hashArquivoSha256,
        CoordenadasGps? coordenadasGps = null,
        DateTime? dataCapturaExif = null)
        : base(Guid.NewGuid())
    {
        if (string.IsNullOrWhiteSpace(urlOriginal))
            throw new RegraNegocioException("URL_ORIGINAL_OBRIGATORIA", "A URL da foto original é obrigatória.");

        if (string.IsNullOrWhiteSpace(urlComprimida))
            throw new RegraNegocioException("URL_COMPRIMIDA_OBRIGATORIA", "A URL da foto comprimida é obrigatória.");

        if (string.IsNullOrWhiteSpace(hashArquivoSha256) || hashArquivoSha256.Length < 32)
            throw new RegraNegocioException("HASH_INVALIDO", "O hash SHA-256 de integridade da imagem é obrigatório.");

        VistoriaId = vistoriaId;
        UrlOriginal = urlOriginal.Trim();
        UrlComprimida = urlComprimida.Trim();
        HashArquivoSha256 = hashArquivoSha256.Trim().ToLowerInvariant();
        CoordenadasGps = coordenadasGps;
        DataCapturaExif = dataCapturaExif;
        DataUploadUtc = DateTime.UtcNow;
    }
}
