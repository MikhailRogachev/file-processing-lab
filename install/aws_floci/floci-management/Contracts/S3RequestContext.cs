namespace floci_management.Contracts;

public class S3RequestContext
{
    public string FileName { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string BucketName { get; set; } = string.Empty;
    public string PackageIdentifier { get; set; } = string.Empty;
    public int Priority { get; set; } = 3;
}
