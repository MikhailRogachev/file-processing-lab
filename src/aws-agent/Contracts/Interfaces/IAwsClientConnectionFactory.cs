using Amazon.S3;

namespace aws_agent.Contracts.Interfaces;

public interface IAwsClientConnectionFactory
{
    string SqsQueueName { get; }
    IAmazonSQS SqsClient();
    IAmazonS3 S3Client();
}
