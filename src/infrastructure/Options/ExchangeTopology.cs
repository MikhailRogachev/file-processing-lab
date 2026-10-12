namespace infrastructure.Options;

public class ExchangeTopology
{
    public string ExchangeName { get; set; } = string.Empty;
    public string QueueName { get; set; } = string.Empty;
    public string[] RoutingKeys { get; set; }
    public string ExchangeType { get; set; } = RabbitMQ.Client.ExchangeType.Topic;
    public string[] Usage { get; set; } = new string[] { };
}
