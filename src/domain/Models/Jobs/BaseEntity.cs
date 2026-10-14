using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace domain.Models.Jobs;

/// <summary>
/// Represents the abstract base class for all domain entities in the processing pipeline.
/// </summary>
/// <remarks>
/// This class serves as the foundational root model for database entities, encapsulating primary key identity,
/// tracking metadata, execution state outcomes, and an in-memory domain event collection used to support
/// the Transactional Outbox pattern.
/// </remarks>
public abstract class BaseEntity
{
    private List<IEvent> _events = new();

    [NotMapped]
    public IReadOnlyCollection<IEvent> Events => _events;

    public Guid Id { get; set; }

    [MaxLength(200)]
    public string Result { get; set; } = string.Empty;

    [MaxLength(800)]
    public string Details { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? UpdatedAt { get; set; }

    public void AddEvents(IEvent @event)
    {
        _events.Add(@event);
    }

    public void RemoveAllEvents()
    {
        _events.Clear();
    }
}
