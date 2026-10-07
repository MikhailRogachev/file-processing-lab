namespace infrastructure.Services.Messaging;

public class RabbitMqPublisher : IRabbitMqPublisher, IDisposable, IAsyncDisposable
{
    private readonly ILogger<RabbitMqPublisher> _logger;
    private readonly MessagingServiceSettings _messagingConfig;
    private readonly SemaphoreSlim _connectionLock = new SemaphoreSlim(1, 1);

    private IConnection? _connection;
    private IChannel? _channel;

    public RabbitMqPublisher(
        ILogger<RabbitMqPublisher> logger,
        IOptions<MessagingServiceSettings> options)
    {
        _logger = logger;
        _messagingConfig = options.Value;
    }

    private BasicProperties GetMsgProperies => new BasicProperties
    {
        ContentType = "application/json",
        DeliveryMode = DeliveryModes.Persistent

    };

    /// <summary>
    /// Asynchronously serializes and publishes a strongly-typed message payload to a specified RabbitMQ exchange.
    /// </summary>
    /// <typeparam name="T">The reference type of the message payload to serialize and transmit.</typeparam>
    /// <param name="message">The strongly-typed message payload instance.</param>
    /// <param name="exchangeName">The target RabbitMQ exchange name responsible for routing the message.</param>
    /// <param name="routingKey">The routing key used by the exchange to evaluate destination queue bindings.</param>
    /// <param name="cancellationToken">A token to observe while awaiting channel initialization and network delivery.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous serialization and transmission operation.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="message"/>, <paramref name="exchangeName"/>, or <paramref name="routingKey"/> is null or empty.
    /// </exception>
    /// <exception cref="System.Text.Json.JsonException">
    /// Thrown when <paramref name="message"/> cannot be serialized to JSON format.
    /// </exception>
    /// <exception cref="RabbitMQ.Client.Exceptions.RabbitMQClientException">
    /// Thrown when an error occurs while communicating with the RabbitMQ broker or when mandatory delivery fails.
    /// </exception>
    /// <remarks>
    /// This method performs the following execution steps:
    /// <list type="number">
    ///   <item>
    ///     <description>Ensures connection and publisher initialization by awaiting <c>EnsurePublisherRegistrationAsync</c>.</description>
    ///   </item>
    ///   <item>
    ///     <description>Serializes <paramref name="message"/> to a UTF-8 JSON byte array using <see cref="System.Text.Json.JsonSerializer"/>.</description>
    ///   </item>
    ///   <item>
    ///     <description>Transmits the message via <c>BasicPublishAsync</c> with mandatory routing (<c>mandatory: true</c>) and persistent delivery headers.</description>
    ///   </item>
    ///   <item>
    ///     <description>Logs an informational trace containing payload type metadata, target exchange, and routing key.</description>
    ///   </item>
    /// </list>
    /// </remarks>
    public async Task PublishAsync<T>(T message, string exchangeName, string routingKey, CancellationToken cancellationToken) where T : class
    {
        await EnsurePublisherRegistrationAsync(cancellationToken);

        var payload = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(payload);

        await _channel!.BasicPublishAsync(
            exchange: exchangeName,
            routingKey: routingKey,
            mandatory: true,
            basicProperties: GetMsgProperies,
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

            var factory = new ConnectionFactory
            {
                HostName = _messagingConfig.RabbitMqConfig.HostName,
                Port = _messagingConfig.RabbitMqConfig.Port,
                UserName = _messagingConfig.RabbitMqConfig.UserName,
                Password = _messagingConfig.RabbitMqConfig.Password,
                VirtualHost = _messagingConfig.RabbitMqConfig.VirtualHost
            };

            _connection = await factory.CreateConnectionAsync(cancellationToken);
            _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

            // register AWS topology
            await RegisterRabbitMqTopologyAsync(_messagingConfig.AwsIngestTology, cancellationToken);

            // register domain topology
            await RegisterRabbitMqTopologyAsync(_messagingConfig.DomainEventsTopology, cancellationToken);

            _logger.LogInformation("RabbitMQ connection established and topologies initialized.");

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


    private async Task RegisterRabbitMqTopologyAsync(ExchangeTopology topology, CancellationToken cancellationToken)
    {
        if (topology == null || string.IsNullOrWhiteSpace(topology.ExchangeName))
        {
            return;
        }

        // register exchange
        await _channel!.ExchangeDeclareAsync(
            exchange: topology.ExchangeName,
            type: topology.ExchangeType,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);

        if (!string.IsNullOrWhiteSpace(topology.QueueName))
        {
            // register queue
            await _channel!.QueueDeclareAsync(
                queue: topology.QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                cancellationToken: cancellationToken);

            // bind queue and exchange
            await _channel.QueueBindAsync(
                queue: topology.QueueName,
                exchange: topology.ExchangeName,
                routingKey: topology.RoutingKey,
                cancellationToken: cancellationToken);

        }

    }


    #endregion

    #region disposal

    public async ValueTask DisposeAsync()
    {
        if (_channel != null)
        {
            await _channel.CloseAsync();
            _channel.Dispose();
        }

        if (_connection != null)
        {
            await _connection.CloseAsync();
            _connection.Dispose();
        }

        _connectionLock.Dispose();
    }

    public void Dispose()
    {
        DisposeAsync().AsTask().GetAwaiter().GetResult();
    }

    #endregion
}
