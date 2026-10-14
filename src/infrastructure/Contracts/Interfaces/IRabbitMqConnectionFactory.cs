namespace infrastructure.Contracts.Interfaces;

public interface IRabbitMqConnectionFactory : IAsyncDisposable, IDisposable
{
    /// <summary>
    /// Gets or creates a thread-safe persistent connection to RabbitMQ.
    /// </summary>
    Task<IConnection> GetConnectionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new dedicated channel and ensures all system exchanges and queues are declared.
    /// </summary>
    Task<IChannel> CreateChannelWithTopologyAsync(CancellationToken cancellationToken = default);
}
