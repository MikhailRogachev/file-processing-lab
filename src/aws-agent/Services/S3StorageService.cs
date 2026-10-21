using Amazon.S3;
using Amazon.S3.Transfer;
using aws_agent.Contracts.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;

namespace aws_agent.Services;

public class S3StorageService : IS3UploadService
{
    private readonly ILogger<S3StorageService> _logger;
    private readonly IAmazonS3 _s3Client;
    private readonly IHubContext<UploadHub> _hubContext;
    private readonly string _bucketName;

    private const long PartSize = 10 * 1024 * 1024;

    public S3StorageService(
        ILogger<S3StorageService> logger,
        IHubContext<UploadHub> hubContext,
        IAwsClientConnectionFactory awsClientFactory,
        IOptions<AwsOptions> options)
    {
        _logger = logger;
        _hubContext = hubContext;
        _s3Client = awsClientFactory.S3Client();
        _bucketName = options.Value.BucketName;
    }


    public Task<bool> CompleteMultipartUploadAsync(string objectKey, string uploadId, List<PartETagDto> parts)
    {
        throw new NotImplementedException();
    }

    public Task<InitiateMultipartResponse> InitiateMultipartUploadAsync(string fileName, long totalSizeBytes, string contentType)
    {
        throw new NotImplementedException();
    }

    public async Task UploadSmallFileAsync(IFormFile file, string s3ObjectKey, string connectionId)
    {
        using var fileStream = file.OpenReadStream();
        var fileTransferUtility = new TransferUtility(_s3Client);
    }
}
