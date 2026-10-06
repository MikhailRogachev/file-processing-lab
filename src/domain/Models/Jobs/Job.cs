namespace domain.Models.Jobs;

public class Job : BaseEntity
{
    public Guid MediaAssetId { get; set; }
    public MediaAsset MediaAsset { get; set; }
    public JobType JobType { get; set; }
    public JobTask Task { get; set; }
    public State State { get; set; }
    public DateTimeOffset? CompletedDate { get; set; }

    protected Job() { }
}