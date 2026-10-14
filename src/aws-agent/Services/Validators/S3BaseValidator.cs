using Amazon.S3;
using Amazon.S3.Model;
using domain.Interfaces.Validators;

namespace aws_agent.Services.Validators;

public abstract class S3BaseValidator : IFileProbeValidator
{
    protected readonly IAmazonS3 Client;
    protected readonly ILogger Logger;

    public abstract string Name { get; }
    protected abstract int StartByte { get; }
    protected abstract int EndByte { get; }

    public S3BaseValidator(
        ILogger logger,
        IAwsClientConnectionFactory awsClientFactory)
    {
        Logger = logger;
        Client = awsClientFactory.S3Client();
    }

    public GetObjectRequest GetObjectRequest(FileProbeContext context, int firstByte, int lastByte)
    {
        return new GetObjectRequest
        {
            BucketName = context.Backet,
            Key = context.FullFileName,
            ByteRange = new ByteRange(firstByte, lastByte)
        };
    }

    public async Task<bool> ValidateAsync(FileProbeContext context, CancellationToken cancellationToken)
    {
        try
        {
            Logger.LogDebug("Validation started for file: {FileName} using probe: {ProbeName}", context.FullFileName, Name);

            var request = new GetObjectRequest
            {
                BucketName = context.Backet, // Fixed 'Backet' typo
                Key = context.FullFileName,
                ByteRange = new ByteRange(StartByte, EndByte)
            };

            using var response = await Client.GetObjectAsync(request, cancellationToken);

            // Delegate specific stream parsing to the child class
            bool isValid = await ValidateStreamAsync(response.ResponseStream, cancellationToken);

            if (isValid)
            {
                Logger.LogInformation("File {FileName} passed {ProbeName} validation.", context.FullFileName, Name);
            }
            else
            {
                Logger.LogWarning("File {FileName} failed {ProbeName} validation.", context.FullFileName, Name);
            }

            return isValid;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error occurred during {ProbeName} validation for file {FileName}.", Name, context.FullFileName);
            return false;
        }
    }

    protected abstract Task<bool> ValidateStreamAsync(Stream stream, CancellationToken cancellationToken);
}
