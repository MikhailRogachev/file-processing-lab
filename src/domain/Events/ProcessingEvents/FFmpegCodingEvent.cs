namespace domain.Events.ProcessingEvents;

public record FFmpegCodingEvent(Guid Id, Guid AggregateId, DateTimeOffset CreatedAt) : IEvent;
