namespace domain.Interfaces.Messaging;

public interface IRabbitMqPublisher
{
    Task PublishAsync<T>(T message, string exchangeName, string routingKey, CancellationToken cancellationToken) where T : class;
}
