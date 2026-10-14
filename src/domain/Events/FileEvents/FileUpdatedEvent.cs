namespace domain.Events.FileEvents;

public record FileUpdatedEvent(Guid Id, Guid ReferenceId, DateTimeOffset CreatedAt, string FileName, string BacketName, long FileSize, string etag) : IEvent;
