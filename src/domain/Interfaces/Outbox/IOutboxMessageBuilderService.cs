using domain.Models.Commands;

namespace domain.Interfaces.Outbox;

public interface IOutboxMessageBuilderService
{
    IEnumerable<Command> Build(IEnumerable<IEvent> eventSource);
}
