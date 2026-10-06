using data.Context;
using domain.Interfaces.Validators;

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
                var service = scope.ServiceProvider.GetRequiredService<IMessageConsumer<AmazonSQSClient>>();
                var validator = scope.ServiceProvider.GetRequiredService<IMediaFileValidator>();
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var messages = await service.ReceiveMessageAsync(stoppingToken);

                if (messages.Any())
                {

                    foreach (var message in messages)
                    {
                        if (!await validator.ValidateMediaFileAsync(message.ObjectKey, stoppingToken))
                        {
                            logger.LogWarning("The file {key} is not allowed to processing.", message.ObjectKey);
                            await service.RemoveMessagesAsync(messages, stoppingToken);

                            continue;
                        }

                        //var mediaFileEntity = new MediaAsset(message.ObjectKey);

                        //context.MediaAssets.Add(mediaFileEntity);
                        //await (context as IUnitOfWork).SaveAsync(stoppingToken);

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
