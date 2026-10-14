using Amazon.S3;

namespace aws_agent.Services;

public class AwsClientConnectionfactory(
    IOptions<AwsOptions> options
    ) : IAwsClientConnectionFactory
{
    public string SqsQueueName => options.Value.SqsQueueName;

    public IAmazonSQS SqsClient()
    {
        return new AmazonSQSClient(
            options.Value.AccessKeyId,
            options.Value.SecretAccessKey,
            new AmazonSQSConfig
            {
                ServiceURL = options.Value.EndPoint,
                AuthenticationRegion = options.Value.Region
            });
    }

    public IAmazonS3 S3Client()
    {
        return new AmazonS3Client(
            options.Value.AccessKeyId,
            options.Value.SecretAccessKey,
            new AmazonS3Config
            {
                ServiceURL = options.Value.EndPoint,
                AuthenticationRegion = options.Value.Region,
                ForcePathStyle = true
            });
    }
}
