namespace domain.Dtos;

public record MessageConsumed(
    string MessageId,
    string ReceiptHandle,
    string EventName,
    string Bucket,
    string FullFileName,
    long FileSize,
    int Priority,
    string Identifier,
    string Etag
    );
