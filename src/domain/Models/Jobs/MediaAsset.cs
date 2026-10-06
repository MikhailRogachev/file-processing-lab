namespace domain.Models.Jobs;

public class MediaAsset : BaseEntity
{
    public Guid MediaPackageId { get; set; }
    public string Filename { get; set; } = string.Empty;
    public ICollection<Job> Jobs { get; set; } = new List<Job>();
    public MediaPackage MediaPackage { get; set; }

    public MediaAsset(Guid mediaPackageId, string fileName)
    {
        Id = Guid.NewGuid();
        MediaPackageId = mediaPackageId;
        Filename = fileName;
    }

    protected MediaAsset() { }
}
