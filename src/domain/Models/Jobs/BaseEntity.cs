using domain.Interfaces.Event;

namespace domain.Models.Jobs;

public class BaseEntity
{
    private List<IEvent> _events = new();
    public IReadOnlyCollection<IEvent> Events => _events;
    public Guid Id { get; set; }
    public string Result { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public void AddEvent(IEvent @event)
    {
        _events.Add(@event);
    }

    public void RemoveAllEvents()
    {
        _events.Clear();
    }

}
