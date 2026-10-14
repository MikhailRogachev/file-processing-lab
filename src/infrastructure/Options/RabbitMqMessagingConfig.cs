namespace infrastructure.Options;

public class RabbitMqMessagingConfig
{
    public RabbitMqConfig RabbitMqConfig { get; set; }
    public Dictionary<string, ExchangeTopology> Topology { get; set; }
}
