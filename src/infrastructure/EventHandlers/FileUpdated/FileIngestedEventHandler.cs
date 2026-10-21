using domain.Dtos;
using Microsoft.Extensions.DependencyInjection;

namespace infrastructure.EventHandlers.FileUpdated;

public class FileIngestedEventHandler(
    ILogger<FileIngestedEventHandler> logger,
    IServiceScopeFactory scopeFactory
    ) : IEventHandler<FileIngestedEvent>
{
    public async Task HandleAsync(FileIngestedEvent @event, CancellationToken cancellationToken)
    {
        logger.LogInformation("Received event FileChanged for the file {fn}", @event.FileName);

        try
        {
            using var scope = scopeFactory.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IMediaPackageRepository>();

            await repository.CreateAsync(new AssetCreateContext(
                @event.Identifier,
                @event.FileName,
                @event.FileSize,
                @event.Priority,
                @event.etag
                ), cancellationToken);
            await repository.UnitOfWork.SaveAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
        }
    }
}
