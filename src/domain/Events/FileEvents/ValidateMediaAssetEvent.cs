namespace domain.Events.FileEvents;

public record ValidateMediaAssetEvent(Guid Id, Guid ReferenceId, DateTimeOffset CreatedAt, string FileName) : IEvent;