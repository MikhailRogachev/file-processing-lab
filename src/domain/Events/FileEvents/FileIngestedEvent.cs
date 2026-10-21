namespace domain.Events.FileEvents;

public record FileIngestedEvent(
    Guid Id,
    Guid AggregateId,
    DateTimeOffset CreatedAt,
    string Identifier,
    string BucketName,
    string FullFileName,
    long FileSize,
    int Priority,
    string etag
    ) : IEvent;
