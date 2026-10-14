using domain.Interfaces.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace infrastructure.Services.OutboxService;

public class OutboxEventPublisher(
    ILogger<OutboxEventPublisher> logger,
    IOptions<RabbitMqMessagingConfig> messagingSettings,
    AppDbContext appDbContext,
    IRabbitMqMessageDeserializer messageDeserializer,
    IServiceScopeFactory scopeFactory
    ) : IOutboxEventPublisher
{

    private readonly string _requestQuery = @"select top(1) [Id]
              ,[ReferenceId]
              ,[Status]
              ,[Content]
              ,[ContentType]
              ,[Repeated]
              ,[CreatedAt]
              ,[LastUpdatedAt]
              ,[ProcessedAt]
              ,[ErrorMessage]
           from [Commands] WITH (UPDLOCK, READPAST) 
           where ProcessedAt IS NULL  
           order by [LastUpdatedAt] ASC";

    private string _exchange_name = string.Empty;

    private string PublisherExchangeName
    {
        get
        {
            if (string.IsNullOrWhiteSpace(_exchange_name))
            {
                var topology = messagingSettings.Value.Topology;
                if (!topology.TryGetValue(ExchangeKeys.COMMAND_BUS, out var exchangeTopology))
                {
                    throw new ArgumentNullException("EventBus topology was not defined.");
                }

                _exchange_name = exchangeTopology.ExchangeName;
            }

            return _exchange_name;
        }
    }


    public async Task RunAsync(CancellationToken cancellationToken)
    {

        await using var transaction = await appDbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var message = await appDbContext.Commands.FromSqlRaw(_requestQuery).FirstOrDefaultAsync();

            if (message == null)
            {
                return;
            }

            var obj = messageDeserializer.Deserialize(message.ContentType, message.Content);

            if (obj == null)
            {
                message.ErrorMessage = "The content type is not recognized. Process is stopped.";
                message.ProcessedAt = DateTimeOffset.UtcNow;
            }
            else
            {
                using var scope = scopeFactory.CreateScope();
                var publisher = scope.ServiceProvider.GetRequiredService<IRabbitMqPublisher>();

                await publisher.PublishAsync(obj, PublisherExchangeName, RoutingKeys.COMMANDS_ROUTE_KEY, cancellationToken);

                appDbContext.Commands.Remove(message);
            }

            await appDbContext.SaveChangesAsync();
            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "The Error occured during command processing: {msg}", ex.Message);
            await transaction.RollbackAsync(cancellationToken);
        }
    }
}
