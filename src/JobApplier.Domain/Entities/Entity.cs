namespace JobApplier.Domain.Entities;

/// <summary>
/// Base entity for all domain entities
/// </summary>
public abstract class Entity
{
    public Guid Id { get; protected set; }
    public DateTime CreatedAt { get; protected set; }
    public DateTime? UpdatedAt { get; protected set; }

    protected Entity()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
    }

    // TODO: Add domain events support
    // private readonly List<DomainEvent> _domainEvents = [];
    // public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();
}
