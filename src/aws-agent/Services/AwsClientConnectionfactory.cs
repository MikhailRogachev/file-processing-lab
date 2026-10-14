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
}
