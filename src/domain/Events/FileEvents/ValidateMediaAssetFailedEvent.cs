namespace domain.Events.FileEvents;

public record ValidateMediaAssetFailedEvent(Guid Id, Guid ReferenceId, DateTimeOffset CreatedAt, string Result, string Details) : IEvent;