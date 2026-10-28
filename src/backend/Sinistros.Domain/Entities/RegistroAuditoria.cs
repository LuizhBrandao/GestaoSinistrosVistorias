using Sinistros.Domain.Common;

namespace Sinistros.Domain.Entities;

/// <summary>
/// Registro imutável de auditoria corporativa para conformidade regulatória (SUSEP/Compliance).
/// </summary>
public sealed class RegistroAuditoria : Entity<Guid>
{
    public Guid SinistroId { get; private set; }
    public DateTime DataHoraUtc { get; private set; }
    public string Usuario { get; private set; } = null!;
    public string Acao { get; private set; } = null!;
    public string Detalhes { get; private set; } = null!;
    public string? IpOrigem { get; private set; }

    private RegistroAuditoria() : base() { }

    public RegistroAuditoria(Guid sinistroId, string usuario, string acao, string detalhes, string? ipOrigem = null)
        : base(Guid.NewGuid())
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(usuario);
        ArgumentException.ThrowIfNullOrWhiteSpace(acao);
        ArgumentException.ThrowIfNullOrWhiteSpace(detalhes);

        SinistroId = sinistroId;
        DataHoraUtc = DateTime.UtcNow;
        Usuario = usuario.Trim();
        Acao = acao.Trim();
        Detalhes = detalhes.Trim();
        IpOrigem = ipOrigem?.Trim();
    }
}
