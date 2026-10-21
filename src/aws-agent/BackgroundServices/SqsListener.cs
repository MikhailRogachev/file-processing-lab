using domain.Events.FileEvents;
using domain.Extensions;
using domain.Interfaces.Event;

namespace aws_agent.Services;

public class SqsListener(
    ILogger<SqsListener> logger,
    IOptions<AwsOptions> options,
    IServiceScopeFactory scopeFactory
    ) : BackgroundService
{

    private readonly TimeSpan _period = TimeSpan.FromSeconds(options.Value.ConsumerRequestPeriodSec);

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


                var messages = await service.ReceiveMessageAsync(stoppingToken);

                if (!messages.IsAny())
                {
                    continue;
                }

                var messagesPublished = new List<MessageConsumed>();

                foreach (var message in messages)
                {
                    try
                    {
                        if (message.EventName == FieldNames.UpdatedEventName)
                        {
                            var handler = scope.ServiceProvider.GetRequiredService<IEventHandler<FileIngestedEvent>>();
                            await handler.HandleAsync(new FileIngestedEvent(
                                Guid.NewGuid(),
                                Guid.NewGuid(),
                                DateTimeOffset.UtcNow,
                                message.Identifier,
                                message.Bucket,
                                message.FullFileName,
                                message.FileSize,
                                message.Priority,
                                message.Etag), stoppingToken);

                            logger.LogDebug("Published message 'FileChanged' for the file {fn}", message.FullFileName);
                        }
                        //else if (message.EventName == FieldNames.DeletedEventName)
                        //{
                        //    var handler = scope.ServiceProvider.GetRequiredService<IEventHandler<FileDeletedEvent>>();
                        //    await handler.HandleAsync(new FileDeletedEvent(
                        //        Guid.NewGuid(),
                        //        Guid.NewGuid(),
                        //        DateTimeOffset.UtcNow,
                        //        message.ObjectKey,
                        //        message.Bucket), stoppingToken);

                        //    logger.LogDebug("Published message 'FileDeleted' for the file {fn}", message.ObjectKey);
                        //}
                        //else
                        //{
                        //    logger.LogDebug("The event received is not defined.");
                        //}

                        messagesPublished.Add(message);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Failed to publish RabbitMQ event {en} for the file {fn} with the identifier {id}",
                            message.EventName,
                            message.FullFileName,
                            message.Identifier);

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
