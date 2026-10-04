using domain.Dtos;

namespace domain.Interfaces.Messaging;

public interface IMessageConsumer<T>
{
    Task<IList<QueueMessageConsumed>> ReceiveMessageAsync(CancellationToken cancellationToken);
    Task RemoveMassagesAsync(IList<QueueMessageConsumed> messages, CancellationToken cancellationToken);
}
