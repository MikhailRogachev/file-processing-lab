namespace domain.Models.Jobs;

public class JobTask : BaseEntity
{
    public Guid JobStageId { get; set; }
    public JobStage JobStage { get; set; }
    public Guid? ParentId { get; set; }
    public JobTask? Parent { get; set; }
    public string Name { get; set; } = string.Empty;
    public State State { get; set; }
    public double Progress { get; set; }
    public int ProgressType { get; set; }
    public int ProgressSteps { get; set; }
    public DateTime? CompletedDate { get; set; }
    public ICollection<JobTask> Children { get; set; } = new List<JobTask>();
}