using domain.Interfaces.Event;

namespace domain.Events;

public class BaseEvent : IEvent
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;

    public BaseEvent()
    {
        Id = Guid.NewGuid();
    }
}
