using Microsoft.Extensions.DependencyInjection;

namespace infrastructure.EventHandlers.FileUpdated;

public class FileChangedEventHandler(
    ILogger<FileChangedEventHandler> logger,
    IServiceScopeFactory scopeFactory
    ) : IEventHandler<FileUpdatedEvent>
{
    public async Task HandleAsync(FileUpdatedEvent @event, CancellationToken cancellationToken)
    {
        logger.LogInformation("Received event FileChanged for the file {fn}", @event.FileName);

        try
        {
            using var scope = scopeFactory.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IMediaPackageRepository>();

            await repository.CreateAsync(@event.FileName, @event.etag, @event.FileSize, cancellationToken);
            await repository.UnitOfWork.SaveAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
        }
    }
}
