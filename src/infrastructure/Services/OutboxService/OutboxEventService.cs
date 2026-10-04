using Microsoft.Extensions.Hosting;

namespace infrastructure.Services.OutboxService;

public class OutboxEventService : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        throw new NotImplementedException();
    }
}
