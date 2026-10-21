namespace aws_agent.Contracts.Interfaces;

public interface IUploadClient
{
    Task ReceiveProgressUpdate(string uploadId, double percentage, long bytesTransferred, long totalBytes);
    Task UploadCompleted(string uploadId, string fileUrl);
    Task UploadFailed(string uploadId, string errorMessage);
}
}
