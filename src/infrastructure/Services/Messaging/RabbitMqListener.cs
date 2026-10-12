using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client.Events;

namespace infrastructure.Services.Messaging;

public class RabbitMqListener : BackgroundService
{
    private readonly ILogger<RabbitMqListener> _logger;
    private readonly RabbitMqMessagingConfig _messagingConfig;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IRabbitMqConnectionFactory _connectionFactory;
    private readonly IRabbitMqMessageDeserializer _messageDeserializer;
    private readonly List<IChannel> _channels = new List<IChannel>();

    public RabbitMqListener(
        ILogger<RabbitMqListener> logger,
        IOptions<RabbitMqMessagingConfig> options,
        IRabbitMqConnectionFactory connectionFactory,
        IRabbitMqMessageDeserializer messageDeserializer,
        IServiceScopeFactory scopeFactory
        )
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        _connectionFactory = connectionFactory;
        _messagingConfig = options.Value;
        _messageDeserializer = messageDeserializer;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting RabbitMQ EventBus...");

        try
        {
            var topology = _messagingConfig.Topology;

            if (topology == null || !topology.Any())
            {
                _logger.LogWarning("No RabbitMQ topology defined. Listener will not start any consumers.");
                return;
            }

            // Extract all valid queue names from the topology configuration
            var queuesToConsume = topology.Values
                .Where(t => !string.IsNullOrWhiteSpace(t.QueueName))
                .Select(t => t.QueueName)
                .Distinct()
                .ToList();

            if (!queuesToConsume.Any())
            {
                _logger.LogWarning("No valid queues found in the topology configuration.");
                return;
            }

            // Launch an isolated consumer task per queue
            var consumerTasks = queuesToConsume
                .Select(queueName => StartConsumerAsync(queueName, stoppingToken));

            _logger.LogInformation("Successfully initialized {Count} queue consumer tasks.", queuesToConsume.Count);

            // Keep all queue consumers running concurrently until cancellation is requested
            await Task.WhenAll(consumerTasks);

        }
        catch (OperationCanceledException)
        {
            _logger.LogError("RabbitMQ EventBus background service is stopping.");
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Fatal error initializing RabbitMQ EventBus: {Message}", ex.Message);
            throw;
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Shutting down RabbitMqListener channels...");

        lock (_channels)
        {
            foreach (var channel in _channels)
            {
                if (channel.IsOpen)
                {
                    // Synchronous close during shutdown to ensure completion
                    channel.CloseAsync(cancellationToken).GetAwaiter().GetResult();
                }
                channel.Dispose();
            }
            _channels.Clear();
        }

        await base.StopAsync(cancellationToken);
    }

    #region private methods and functions

    private async Task StartConsumerAsync(string queueName, CancellationToken cancellationToken)
    {
        try
        {
            var channel = await _connectionFactory.CreateChannelWithTopologyAsync(cancellationToken);

            lock (_channels)
            {
                _channels.Add(channel);
            }

            await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 10, global: false, cancellationToken: cancellationToken);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (model, msg) =>
            {
                var bodyMessageType = msg.BasicProperties.Type;
                var bodyBytes = msg.Body.ToArray();
                var messageJson = Encoding.UTF8.GetString(bodyBytes);
                var routingKey = msg.RoutingKey;
                var isSuccess = false;

                _logger.LogDebug("Received message on queue '{Queue}' with RoutingKey '{RoutingKey}'", queueName, routingKey);

                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    isSuccess = await DispatchToHandlerAsync(scope, queueName, routingKey, messageJson, bodyMessageType, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unhandled exception processing message on queue '{Queue}' for RoutingKey '{RoutingKey}'",
                        queueName,
                        routingKey);
                    isSuccess = false;
                }

                if (isSuccess)
                {
                    // Acknowledge receipt to clear message from queue
                    await channel.BasicAckAsync(
                        deliveryTag: msg.DeliveryTag,
                        multiple: false,
                        cancellationToken: cancellationToken);
                }
                else
                {
                    // Reject message without requeue (sends to Dead Letter Exchange if configured)
                    await channel.BasicNackAsync(
                        deliveryTag: msg.DeliveryTag,
                        multiple: false,
                        requeue: false,
                        cancellationToken: cancellationToken);
                }
            };

            await channel.BasicConsumeAsync(
                queue: queueName,
                autoAck: false,
                consumer: consumer,
                cancellationToken: cancellationToken);

            _logger.LogInformation("Active consumer listening on queue: {QueueName}", queueName);

            // Suspend task execution to keep consumer channel alive for the queue
            // Task.Delay(Timeout.Infinite): This tells the runtime to create a timer
            // that never expires. If you ran this without a cancellation token, the
            // thread/task would just hang there permanently. It is the asynchronous
            // equivalent of an infinite loop, but it yields the thread back to the
            // thread pool so it costs zero CPU resources.
            await Task.Delay(Timeout.Infinite, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogError("Consumer task for queue '{QueueName}' cancelled.", queueName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred in consumer task for queue '{QueueName}'", queueName);
        }
    }

    private async Task<bool> DispatchToHandlerAsync(
        IServiceScope scope,
        string queueName,
        string routingKey,
        string messageJson,
        string? messageType,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(messageType))
        {
            _logger.LogDebug("The message received in the queue {name} / routing key {rk} hasn't message type attribute",
                queueName,
                routingKey);
            return false;
        }

        var eventType = _messageDeserializer.ResolveType(messageType);
        if (eventType == null)
        {
            _logger.LogDebug("The type is not found for the message type {mst}", messageType);
            return false;
        }

        var eventData = _messageDeserializer.Deserialize(messageType, messageJson);
        if (eventData == null)
        {
            _logger.LogDebug("The message can't be desirialised forthe message type {mst}", messageType);
            return false;
        }

        Type handlerInterfaceType = typeof(IEventHandler<>).MakeGenericType(eventType);

        try
        {
            var handler = scope.ServiceProvider.GetRequiredKeyedService(handlerInterfaceType, routingKey);
            await ((dynamic)handler).HandleAsync((dynamic)eventData, cancellationToken);

            return true;
        }
        catch (InvalidOperationException)
        {
            _logger.LogWarning("No keyed handler registered for RoutingKey '{RoutingKey}' and Type '{Type}'", routingKey, messageType);

            return true; // ACK unhandled messages to prevent blocking the queue
        }
    }

    #endregion

}
