using infrastructure.Contracts.Interfaces;

namespace infrastructure.Services.Messaging;

public class RabbitMqPublisher : IRabbitMqPublisher, IDisposable, IAsyncDisposable
{
    private readonly ILogger<RabbitMqPublisher> _logger;
    private readonly IRabbitMqConnectionFactory _connectionFactory;
    private readonly SemaphoreSlim _connectionLock = new SemaphoreSlim(1, 1);

    private IChannel? _channel;

    public RabbitMqPublisher(
        ILogger<RabbitMqPublisher> logger,
        IRabbitMqConnectionFactory connectionFactory)
    {
        _logger = logger;
        _connectionFactory = connectionFactory;
    }



    public async Task PublishAsync<T>(T message, string exchangeName, string routingKey, CancellationToken cancellationToken) where T : class
    {
        await EnsurePublisherRegistrationAsync(cancellationToken);

        var payload = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(payload);

        await _channel!.BasicPublishAsync(
            exchange: exchangeName,
            routingKey: routingKey,
            mandatory: true,
            basicProperties: GetMsgProperies<T>(),
            body: body,
            cancellationToken: cancellationToken);

        _logger.LogInformation(
            "Published {Type} to Exchange '{Exchange}' with RoutingKey '{RoutingKey}'",
            typeof(T).Name, exchangeName, routingKey);
    }

    #region private methods and functions

    private async Task EnsurePublisherRegistrationAsync(CancellationToken cancellationToken)
    {
        if (_channel is { IsOpen: true })
        {
            return;
        }

        await _connectionLock.WaitAsync(cancellationToken);

        try
        {
            if (_channel is { IsOpen: true })
            {
                return;
            }


            _channel = await _connectionFactory.CreateChannelWithTopologyAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "The error occured during establishing rabbitMq exchange: {msg}", ex.Message);
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    private BasicProperties GetMsgProperies<T>() => new BasicProperties
    {
        ContentType = "application/json",
        DeliveryMode = DeliveryModes.Persistent,
        Type = typeof(T).FullName
    };


    #endregion

    #region disposal

    public async ValueTask DisposeAsync()
    {
        if (_channel != null)
        {
            await _channel.CloseAsync();
            _channel.Dispose();
        }

        _connectionLock.Dispose();
    }

    public void Dispose()
    {
        DisposeAsync().AsTask().GetAwaiter().GetResult();
    }

    #endregion
}
