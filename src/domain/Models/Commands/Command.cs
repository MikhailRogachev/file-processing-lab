using System.Text.Json;

namespace domain.Models.Commands;

public class Command
{
    public Guid Id { get; set; }
    public CommandStatus Status { get; set; }
    public JsonDocument Content { get; set; }
    public string ContentType { get; set; }
    public int Repeated { get; set; } = 0;
    public DateTime CreatedAt { get; set; }
}
