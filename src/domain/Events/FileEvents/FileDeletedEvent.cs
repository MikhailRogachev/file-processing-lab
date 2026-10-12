namespace domain.Events.FileEvents;

public record class FileDeletedEvent(Guid EventId, DateTimeOffset CreatedAt, string FileName, string BaketName) : IEvent;
