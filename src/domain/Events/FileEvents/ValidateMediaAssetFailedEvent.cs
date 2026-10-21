namespace domain.Events.FileEvents;

public record ValidateMediaAssetFailedEvent(Guid Id, Guid AggregateId, DateTimeOffset CreatedAt, string Result, string Details) : IEvent;