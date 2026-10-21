using aws_agent.Contracts.Dtos;
using Microsoft.AspNetCore.Http;

namespace aws_agent.Contracts.Interfaces;

public interface IS3UploadService
{
    Task UploadSmallFileAsync(IFormFile file, string s3ObjectKey, string connectionId);
    Task<InitiateMultipartResponse> InitiateMultipartUploadAsync(string fileName, long totalSizeBytes, string contentType);
    Task<bool> CompleteMultipartUploadAsync(string objectKey, string uploadId, List<PartETagDto> parts);
}
