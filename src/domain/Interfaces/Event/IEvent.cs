namespace domain.Interfaces.Event;

public interface IEvent
{
    public Guid Id { get; }
    public Guid ReferenceId { get; }
    public DateTimeOffset CreatedAt { get; }
}
