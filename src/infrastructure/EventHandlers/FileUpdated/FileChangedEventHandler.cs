using Microsoft.Extensions.DependencyInjection;

namespace infrastructure.EventHandlers.FileUpdated;

public class FileChangedEventHandler(
    ILogger<FileChangedEventHandler> logger,
    IServiceScopeFactory scopeFactory
    //IMediaPackageRepository repository,
    //IRabbitMqPublisher publisher
    ) : IEventHandler<FileUpdatedEvent>, IEventHandler<FileDeletedEvent>
{
    public async Task HandleAsync(FileUpdatedEvent @event, CancellationToken cancellationToken)
    {
        logger.LogInformation("Received event FileChanged for the file {fn}", @event.FileName);

        try
        {
            using var scope = scopeFactory.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IMediaPackageRepository>();

            await repository.CreateAsync(@event.FileName, cancellationToken);
            await repository.UnitOfWork.SaveAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
        }




        //await publisher.PublishAsync(
        //exchangeName: PublisherExchangeName,
        //routingKey: RoutingKeys.AWS_FILECHANGED,
        //message: new FileUpdatedEvent(
        //    Guid.NewGuid(),
        //    DateTimeOffset.UtcNow,
        //    message.ObjectKey,
        //    message.Bucket,
        //    message.ObjectSize
        //),
        //cancellationToken: stoppingToken
        //);
    }

    public Task HandleAsync(FileDeletedEvent @event, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
