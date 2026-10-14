namespace aws_file_validation.Services;

public class FileProbe
{
    public string S3Bucket { get; set; } = string.Empty;
    public string S3Key { get; set; } = string.Empty;
    public string Extension { get; set; } = string.Empty;
    public string TempFilePath { get; set; } = string.Empty;
}

public interface IFileProbe
{
    string Name { get; }
    Task<bool> ValidateAsync(FileProbe context, CancellationToken cancellationToken);
}

public record FileItem(int index, string filename, long? filesize);
