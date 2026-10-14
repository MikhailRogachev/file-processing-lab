

using Microsoft.Extensions.Hosting;

namespace floci_management.BackgroundServices;

public class SqsListener : BackgroundService
{
    private readonly TimeSpan _period = TimeSpan.FromSeconds(3);
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using PeriodicTimer periodicTimer = new PeriodicTimer(_period);

        while (await periodicTimer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                Console.WriteLine("Working");

            }
            catch (Exception ex)
            {
                Console.WriteLine("Sqs listener: {0}", ex.Message);
            }
        }
    }
}
