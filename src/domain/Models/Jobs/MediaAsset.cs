namespace domain.Models.Jobs;

public class MediaAsset : BaseEntity
{
    public string Filename { get; set; } = string.Empty;
    public AssetType AssetType { get; set; }
    public long Size { get; set; }
    public string Etag { get; set; }
    public int Version { get; set; }
    public ICollection<Job> Jobs { get; set; } = new List<Job>();
    public Guid MediaPackageId { get; set; }
    public MediaPackage MediaPackage { get; set; }

    public MediaAsset(Guid mediaPackageId, string fileName)
    {
        Id = Guid.NewGuid();
        MediaPackageId = mediaPackageId;
        Filename = fileName;
    }

    protected MediaAsset() { }
}
