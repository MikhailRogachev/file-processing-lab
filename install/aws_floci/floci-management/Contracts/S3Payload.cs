namespace floci_management.Contracts;

public class S3Payload<T>
{
    public bool IsSuccess { get; set; }
    public T? Context { get; set; }
    public string Message { get; set; } = string.Empty;
}
