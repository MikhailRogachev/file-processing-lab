namespace aws_agent.Options;
public class AwsOptions
{
    public string EndPoint { get; set; } = string.Empty;
    public string AccessKeyId { get; set; } = string.Empty;
    public string SecretAccessKey { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public int ConsumerRequestPeriodSec { get; set; }
    public string BucketName { get; set; } = string.Empty;
    public string SqsQueueName { get; set; } = string.Empty;
}
