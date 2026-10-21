namespace domain.Interfaces.Event;

public interface IEvent
{
    public Guid Id { get; }
    public Guid AggregateId { get; }
    public DateTimeOffset CreatedAt { get; }
}
