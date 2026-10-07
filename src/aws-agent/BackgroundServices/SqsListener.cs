using infrastructure.Options;

namespace aws_agent.Services;

public class SqsListener(
    ILogger<SqsListener> logger,
    IOptions<AwsOptions> options,
    IOptions<MessagingServiceSettings> messagingSettings,
    IServiceScopeFactory scopeFactory
    ) : BackgroundService
{

    private readonly TimeSpan _period = TimeSpan.FromSeconds(options.Value.ConsumerRequestPeriodSec);

    private string PublisherExchangeName => messagingSettings.Value.AwsIngestTology.ExchangeName;

    private string PublisherRoutingKey => messagingSettings.Value.AwsIngestTology.RoutingKey;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using PeriodicTimer periodicTimer = new PeriodicTimer(_period);

        logger.LogInformation("Starting SQS Listener...");

        while (await periodicTimer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                using IServiceScope scope = scopeFactory.CreateScope();
                var service = scope.ServiceProvider.GetRequiredService<IMessageConsumer<AmazonSQSClient>>();
                var publisher = scope.ServiceProvider.GetRequiredService<IRabbitMqPublisher>();

                var messages = await service.ReceiveMessageAsync(stoppingToken);

                if (messages.Any())
                {
                    foreach (var message in messages)
                    {
                        // PUBLISH MESSAGE TO RABBIT MQ FOR NEXT STEP
                        await publisher.PublishAsync(
                            exchangeName: PublisherExchangeName,
                            routingKey: PublisherRoutingKey,
                            message: message,
                            cancellationToken: stoppingToken
                            );

                        //Delete messages only after successful processing/publishing,
                        //and delete them in bulk once after the loop completes (or individually on success).
                        await service.RemoveMessagesAsync(messages, stoppingToken);
                    }

                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Sqs listener error: {msg}", ex.Message);
            }
        }
    }
}
