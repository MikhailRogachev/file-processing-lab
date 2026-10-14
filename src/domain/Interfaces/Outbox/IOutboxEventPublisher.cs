namespace domain.Interfaces.Outbox;

public interface IOutboxEventPublisher
{
    Task RunAsync(CancellationToken cancellationToken);
}
