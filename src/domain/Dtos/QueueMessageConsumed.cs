namespace domain.Dtos;

public record QueueMessageConsumed(
    string MessageId,
    string ReceiptHandle,
    string EventName,
    string Bucket,
    string ObjectKey,
    long ObjectSize
    );
