namespace domain.Dtos;

public class MessageConsumed
{
    public string MessageId { get; set; } = string.Empty;
    public string ReceiptHandle { get; set; } = string.Empty;
    public string EventName { get; set; } = string.Empty;
    public string Bucket { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string Identifier { get; set; } = string.Empty;
    public string Etag { get; set; } = string.Empty;
    public int Priority { get; set; }
    public long FileSize { get; set; }
}
