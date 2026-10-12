namespace domain.Events.FileEvents;

public record FileUpdatedEvent(Guid EventId, DateTimeOffset CreatedAt, string FileName, string BacketName, long FileSize) : IEvent;
