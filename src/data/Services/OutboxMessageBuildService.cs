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
                ReferenceId = @event.Id,
                Status = CommandStatus.Queued,
                Content = JsonSerializer.Serialize(@event, @event.GetType()),
                ContentType = @event.GetType().FullName ?? string.Empty,
                Repeated = 0,
                CreatedAt = @event.CreatedAt,
                LastUpdatedAt = DateTime.UtcNow,
            };
            outboxMessages.Add(outboxMessage);

            logger.LogDebug("The event type is {tp} added to outbox collection.", @event.GetType().Name);
        }

        return outboxMessages;
    }
}
