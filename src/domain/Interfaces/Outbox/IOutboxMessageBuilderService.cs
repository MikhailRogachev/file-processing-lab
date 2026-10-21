namespace domain.Interfaces.Outbox;

public interface IOutboxMessageBuilderService
{
    IEnumerable<OutboxMessage> Build(IEnumerable<IEvent> eventSource);
}
