using domain.Contracts.Enums;
using domain.Extensions;
using domain.Interfaces.Event;
using domain.Interfaces.Outbox;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace data.Services;

public class OutboxMessageBuildService(ILogger<OutboxMessageBuildService> logger) : IOutboxMessageBuilderService
{
    public IEnumerable<Command> Build(IEnumerable<IEvent> eventSource)
    {
        var outboxMessages = new List<Command>();

        if (!eventSource.IsAny())
            return outboxMessages;

        foreach (var @event in eventSource)
        {
            var outboxMessage = new Command
            {
                Id = Guid.NewGuid(),
                Status = CommandStatus.Created,
                Content = JsonSerializer.SerializeToDocument(@event, @event.GetType()),
                ContentType = @event.GetType().FullName ?? string.Empty,
                Repeated = 0,
                CreatedAt = @event.CreatedAt
            };
            outboxMessages.Add(outboxMessage);

            logger.LogDebug("The event type is {tp} added to outbox collection.", @event.GetType().Name);
        }

        return outboxMessages;
    }
}
