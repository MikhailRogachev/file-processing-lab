namespace domain.Models.Jobs;

public class JobStage : BaseEntity
{
    public Guid JobId { get; set; }
    public Job Job { get; set; }
    public int StageType { get; set; } // e.g., Preprocess, Process, Subprocess
    public State State { get; set; }
    public double Progress { get; set; }
    public DateTime? CompletedDate { get; set; }
    public ICollection<JobTask> Tasks { get; set; } = new List<JobTask>();
}