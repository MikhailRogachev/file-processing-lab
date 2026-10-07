namespace domain.Models.Commands;

public class Command
{
    public Guid Id { get; set; }
    public Guid? ReferenceId { get; set; }
    public CommandStatus Status { get; set; }
    public string Content { get; set; }
    public string ContentType { get; set; }
    public int Repeated { get; set; } = 0;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset LastUpdatedAt { get; set; }
    public DateTimeOffset? ProcessedAt { get; set; }
    public string ErorMessage { get; set; } = string.Empty;
}
