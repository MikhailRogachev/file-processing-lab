namespace infrastructure.EventHandlers.FileUpdated;

public class FileValidateEventHandler(
    ILogger<FileValidateEventHandler> logger
    ) : IEventHandler<ValidateMediaAssetEvent>
{

    public async Task HandleAsync(ValidateMediaAssetEvent @event, CancellationToken cancellationToken)
    {
        logger.LogDebug("Validating Asset {fn}", @event.FileName);

        await Task.CompletedTask;
    }
}
