namespace Sinistros.Domain.Common;

/// <summary>
/// Classe base para Entidades DDD, provendo identidade única imutável e igualdade por ID.
/// </summary>
public abstract class Entity<TId> : IEquatable<Entity<TId>> where TId : notnull
{
    public TId Id { get; protected set; } = default!;

    protected Entity(TId id)
    {
        if (EqualityComparer<TId>.Default.Equals(id, default!))
            throw new ArgumentException("O identificador da entidade não pode ser nulo ou valor padrão.", nameof(id));

        Id = id;
    }

    // Construtor sem parâmetros para ORM (EF Core)
    protected Entity() { }

    public bool Equals(Entity<TId>? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (GetType() != other.GetType()) return false;
        return EqualityComparer<TId>.Default.Equals(Id, other.Id);
    }

    public override bool Equals(object? obj) => obj is Entity<TId> other && Equals(other);

    public override int GetHashCode() => EqualityComparer<TId>.Default.GetHashCode(Id);

    public static bool operator ==(Entity<TId>? left, Entity<TId>? right) => Equals(left, right);
    public static bool operator !=(Entity<TId>? left, Entity<TId>? right) => !Equals(left, right);
}
