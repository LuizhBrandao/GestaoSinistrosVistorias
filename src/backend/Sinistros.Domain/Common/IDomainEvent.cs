namespace Sinistros.Domain.Common;

/// <summary>
/// Interface marcadora para eventos de domínio disparados por agregados.
/// </summary>
public interface IDomainEvent
{
    Guid EventId { get; }
    DateTime OcorreuEmUtc { get; }
}

public abstract record DomainEventBase : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OcorreuEmUtc { get; init; } = DateTime.UtcNow;
}
