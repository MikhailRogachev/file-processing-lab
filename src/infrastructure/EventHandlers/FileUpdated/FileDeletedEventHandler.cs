using domain.Events.FileEvents;

namespace infrastructure.EventHandlers.FileUpdated;

public class FileDeletedEventHandler(
    ILogger<FileDeletedEventHandler> logger
    ) : IEventHandler<FileDeletedEvent>
{
    public async Task HandleAsync(FileDeletedEvent @event, CancellationToken cancellationToken)
    {
        logger.LogInformation("Start Defete file handler");
    }
}
