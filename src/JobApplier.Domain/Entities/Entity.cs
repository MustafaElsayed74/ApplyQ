namespace JobApplier.Domain.Entities;

/// <summary>
/// Base entity for all domain entities with soft delete support
/// </summary>
public abstract class Entity
{
    public Guid Id { get; protected set; }
    public DateTime CreatedAt { get; protected set; }
    public DateTime? UpdatedAt { get; protected set; }

    /// <summary>
    /// Soft delete timestamp. Null if entity is active.
    /// </summary>
    public DateTime? DeletedAt { get; protected set; }

    protected Entity()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        DeletedAt = null;
    }

    /// <summary>
    /// Soft delete this entity
    /// </summary>
    public virtual void Delete()
    {
        DeletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Restore a soft-deleted entity
    /// </summary>
    public virtual void Restore()
    {
        DeletedAt = null;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Check if entity is soft deleted
    /// </summary>
    public bool IsDeleted => DeletedAt.HasValue;

    // TODO: Add domain events support
    // private readonly List<DomainEvent> _domainEvents = [];
    // public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();
}
