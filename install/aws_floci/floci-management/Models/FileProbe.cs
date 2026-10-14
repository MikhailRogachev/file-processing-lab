namespace floci_management.Models;

public class FileProbe
{
    public string S3Bucket { get; set; } = string.Empty;
    public string S3Key { get; set; } = string.Empty;
    public string Extension { get; set; } = string.Empty;
    public string TempFilePath { get; set; } = string.Empty;
}
