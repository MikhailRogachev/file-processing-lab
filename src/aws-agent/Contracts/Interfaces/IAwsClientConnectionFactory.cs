namespace aws_agent.Contracts.Interfaces;

public interface IAwsClientConnectionFactory
{
    string SqsQueueName { get; }
    IAmazonSQS SqsClient();
}
