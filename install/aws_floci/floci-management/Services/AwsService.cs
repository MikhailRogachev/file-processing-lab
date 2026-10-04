

namespace floci_management.Services;

public class AwsService
{
    protected AmazonS3Config S3Config => new AmazonS3Config
    {
        ServiceURL = Settings.AWS_ENDPOINT_URL,
        AuthenticationRegion = Settings.AWS_DEFAULT_REGION,
        ForcePathStyle = true
    };

    protected AmazonSQSConfig SqsConfig => new AmazonSQSConfig
    {
        ServiceURL = Settings.AWS_ENDPOINT_URL,
        AuthenticationRegion = Settings.AWS_DEFAULT_REGION
    };

    protected AmazonS3Client S3Client() => new AmazonS3Client(Settings.AWS_ACCESS_KEY_ID, Settings.AWS_SECRET_ACCESS_KEY, S3Config);

    protected AmazonSQSClient SqsClient() => new AmazonSQSClient(Settings.AWS_ACCESS_KEY_ID, Settings.AWS_SECRET_ACCESS_KEY, SqsConfig);
}
