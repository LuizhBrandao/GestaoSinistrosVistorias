namespace Sinistros.Domain.Common;

/// <summary>
/// Interface marcadora para Raízes de Agregação (Aggregate Roots).
/// </summary>
public interface IAggregateRoot { }

/// <summary>
/// Classe base para Raízes de Agregação com suporte a disparo e acúmulo de Eventos de Domínio.
/// </summary>
public abstract class AggregateRoot<TId> : Entity<TId>, IAggregateRoot where TId : notnull
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected AggregateRoot(TId id) : base(id) { }

    protected AggregateRoot() : base() { }

    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents() => _domainEvents.Clear();
}
