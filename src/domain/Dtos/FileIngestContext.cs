namespace domain.Dtos;

public class FileIngestContext
{
    public string FileName { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string BucketName { get; set; } = string.Empty;
    public string PackageIdentifier { get; set; } = string.Empty;
    public int Priority { get; set; } = 3;

    public string FileFullName()
    {
        return System.IO.Path.Combine(Path, FileName);
    }
}
