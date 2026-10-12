namespace domain.Events.FileEvents;

public record class FileDeletedEvent(Guid Id, Guid ReferenceId, DateTimeOffset CreatedAt, string FileName, string BaketName) : IEvent;
