namespace aws_agent.Contracts.Interfaces;

public interface IMessageConsumer
{
    Task<IList<QueueMessageConsumed>> ReceiveMessageAsync(CancellationToken cancellationToken);
    Task RemoveMessagesAsync(List<QueueMessageConsumed> messages, CancellationToken cancellationToken);
}
