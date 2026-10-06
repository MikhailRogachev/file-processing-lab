namespace domain.Interfaces.Event;

public interface IEvent
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
