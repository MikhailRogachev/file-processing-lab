namespace infrastructure.EventHandlers.FileUpdated;

public class FileDeletedEventHandler : IEventHandler<FileDeletedEvent>
{
    public Task HandleAsync(FileDeletedEvent @event, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
