using domain.Contracts.Contants;
using domain.Events;
using domain.Events.FileEvents;
using domain.Extensions;
using infrastructure.Contracts.Interfaces;
using infrastructure.Options;

namespace aws_agent.Services;

public class SqsListener(
    ILogger<SqsListener> logger,
    IOptions<AwsOptions> options,
    IOptions<RabbitMqMessagingConfig> messagingSettings,
    IServiceScopeFactory scopeFactory
    ) : BackgroundService
{

    private readonly TimeSpan _period = TimeSpan.FromSeconds(options.Value.ConsumerRequestPeriodSec);
    private string _exchange_name = string.Empty;

    private string PublisherExchangeName
    {
        get
        {
            if (string.IsNullOrWhiteSpace(_exchange_name))
            {
                var topology = messagingSettings.Value.Topology;
                if (!topology.TryGetValue(ExchangeKeys.EVENT_BUS, out var exchangeTopology))
                {
                    throw new ArgumentNullException("EventBus topology was not defined.");
                }

                _exchange_name = exchangeTopology.ExchangeName;
            }

            return _exchange_name;
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using PeriodicTimer periodicTimer = new PeriodicTimer(_period);

        logger.LogInformation("Starting SQS Listener...");

        while (await periodicTimer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                using IServiceScope scope = scopeFactory.CreateScope();
                var service = scope.ServiceProvider.GetRequiredService<IMessageConsumer>();
                var publisher = scope.ServiceProvider.GetRequiredService<IRabbitMqPublisher>();

                var messages = await service.ReceiveMessageAsync(stoppingToken);

                if (!messages.IsAny())
                {
                    continue;
                }

                var messagesPublished = new List<QueueMessageConsumed>();

                foreach (var message in messages)
                {
                    try
                    {
                        if (message.EventName == FieldNames.UpdatedEventName)
                        {
                            await publisher.PublishAsync(
                                exchangeName: PublisherExchangeName,
                                routingKey: RoutingKeys.AWS_FILECHANGED,
                                message: new FileUpdatedEvent(
                                    Guid.NewGuid(),
                                    DateTimeOffset.UtcNow,
                                    message.ObjectKey,
                                    message.Bucket,
                                    message.ObjectSize
                                ),
                                cancellationToken: stoppingToken
                                );

                            logger.LogDebug("Published message 'FileChanged' for the file {fn}", message.ObjectKey);
                        }
                        else if (message.EventName == FieldNames.DeletedEventName)
                        {
                            await publisher.PublishAsync(
                                exchangeName: PublisherExchangeName,
                                routingKey: RoutingKeys.AWS_FILEDELETED,
                                message: new FileDeletedEvent(
                                    Guid.NewGuid(),
                                    DateTimeOffset.UtcNow,
                                    message.ObjectKey,
                                    message.Bucket
                                ),
                                cancellationToken: stoppingToken
                                );

                            logger.LogDebug("Published message 'FileDeleted' for the file {fn}", message.ObjectKey);
                        }
                        else
                        {
                            logger.LogDebug("The event received is not defined.");
                        }

                        messagesPublished.Add(message);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Failed to publish RabbitMQ event {en} for the file {fn}",
                            message.EventName,
                            message.ObjectKey);

                    }
                }

                // remove from the queue all messages suvccessfully published
                if (messagesPublished.IsAny())
                {
                    await service.RemoveMessagesAsync(messagesPublished, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unhandled error during SQS polling execution loop: {msg}", ex.Message);
            }
        }
    }
}
