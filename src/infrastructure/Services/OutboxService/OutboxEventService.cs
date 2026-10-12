using domain.Interfaces.Outbox;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace infrastructure.Services.OutboxService;

public class OutboxEventService(
    ILogger<OutboxEventService> logger,
    IOptions<OutboxMessageOptions> options,
    IServiceScopeFactory scopeFactory
    ) : BackgroundService
{
    private int RequestTimeout => options.Value.ScanIntervalMs;


    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("{time} The Outbox Service is starting.", DateTime.UtcNow);

        stoppingToken.Register(() => logger.LogInformation("{time} Cancelling the Outbox Service due to host shutdown", DateTime.UtcNow));

        using (var scope = scopeFactory.CreateScope())
        {
            IOutboxEventPublisher publisher = scope.ServiceProvider.GetService<IOutboxEventPublisher>()!;

            while (!stoppingToken.IsCancellationRequested)
            {
                await publisher.RunAsync(stoppingToken);
                await Task.Delay(RequestTimeout, stoppingToken);
            }
        }
    }
}
