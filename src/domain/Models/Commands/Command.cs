namespace domain.Models.Commands;

public class Command
{
    public Guid Id { get; set; }
    public Guid ReferenceId { get; set; }
    public CommandStatus Status { get; set; }
    public string Content { get; set; }
    public string ContentType { get; set; }
    public int Repeated { get; set; } = 0;
    public DateTime CreatedAt { get; set; }
    public DateTime LastUpdatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public string ErorMessage { get; set; } = string.Empty;
}
