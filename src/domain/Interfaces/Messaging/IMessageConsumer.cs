using domain.Dtos;

namespace domain.Interfaces.Messaging;

public interface IMessageConsumer<T>
{
    Task<IList<QueueMessageConsumed>> ReceiveMessageAsync(CancellationToken cancellationToken);
    Task RemoveMessagesAsync(IList<QueueMessageConsumed> messages, CancellationToken cancellationToken);
}
