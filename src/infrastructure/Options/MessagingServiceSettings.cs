namespace infrastructure.Options;

public class MessagingServiceSettings
{
    public RabbitMqConfig RabbitMqConfig { get; set; }

    public ExchangeTopology AwsIngestTology { get; set; }

    public ExchangeTopology DomainEventsTopology { get; set; }
}
