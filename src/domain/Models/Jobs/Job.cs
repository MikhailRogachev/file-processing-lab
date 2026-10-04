namespace domain.Models.Jobs;

public class Job : BaseEntity
{
    public Guid MediaAssetId { get; set; }
    public MediaAsset MediaAsset { get; set; }
    public int JobType { get; set; }
    public string? Filename { get; set; }
    public State State { get; set; }
    public DateTime? CompletedDate { get; set; }

    // Relational navigation
    public ICollection<JobStage> Stages { get; set; } = new List<JobStage>();
}