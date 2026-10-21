using domain.Contracts.Enums;
using domain.Extensions;
using domain.Interfaces.Event;
using domain.Interfaces.Outbox;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace data.Services;

public class OutboxMessageBuildService(ILogger<OutboxMessageBuildService> logger) : IOutboxMessageBuilderService
{
    public IEnumerable<OutboxMessage> Build(IEnumerable<IEvent> eventSource)
    {
        var outboxMessages = new List<OutboxMessage>();

        if (!eventSource.IsAny())
            return outboxMessages;

        foreach (var @event in eventSource)
        {
            var outboxMessage = new OutboxMessage
            {
                Id = @event.Id,
                ReferenceId = @event.AggregateId,
                Status = OutboxMessageStatus.Queued,
                Content = JsonSerializer.Serialize(@event, @event.GetType()),
                ContentType = @event.GetType().FullName ?? string.Empty,
                Repeated = 0,
                CreatedAt = DateTimeOffset.UtcNow,
                LastUpdatedAt = DateTimeOffset.UtcNow,
            };
            outboxMessages.Add(outboxMessage);

            logger.LogDebug("The event type is {tp} added to outbox collection.", @event.GetType().Name);
        }

        return outboxMessages;
    }
}
