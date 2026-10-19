namespace domain.Events.ProcessingEvents;

public record FFmpegCodingEvent(Guid Id, Guid ReferenceId, DateTimeOffset CreatedAt) : IEvent
