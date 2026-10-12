namespace domain.Interfaces.Event;

public interface IEvent
{
    public Guid EventId { get; }
    public DateTimeOffset CreatedAt { get; }
}
