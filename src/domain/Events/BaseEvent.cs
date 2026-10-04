using domain.Interfaces.Event;

namespace domain.Events;

public class BaseEvent : IEvent
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }

    public BaseEvent()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
    }
}
