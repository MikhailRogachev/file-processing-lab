namespace aws_agent.Contracts.Interfaces;

public interface IMessageConsumer
{
    Task<IList<MessageConsumed>> ReceiveMessageAsync(CancellationToken cancellationToken);
    Task RemoveMessagesAsync(List<MessageConsumed> messages, CancellationToken cancellationToken);
}
