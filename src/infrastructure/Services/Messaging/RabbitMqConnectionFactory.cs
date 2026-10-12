using infrastructure.Contracts.Interfaces;

namespace infrastructure.Services.Messaging;

public class RabbitMqConnectionFactory(
    ILogger<RabbitMqConnectionFactory> logger,
    IOptions<RabbitMqMessagingConfig> options
    ) : IRabbitMqConnectionFactory
{
    private readonly SemaphoreSlim _connectionLocker = new SemaphoreSlim(1, 1);

    private IConnection? _connection;
    private bool _topologyInitialized;

    private RabbitMqConfig RabbitMqConfig => options.Value.RabbitMqConfig;


    public async Task<IChannel> CreateChannelWithTopologyAsync(CancellationToken cancellationToken = default)
    {
        var connection = await GetConnectionAsync(cancellationToken);
        var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        if (!_topologyInitialized)
        {
            await _connectionLocker.WaitAsync(cancellationToken);

            try
            {
                if (_topologyInitialized)
                {
                    return channel;
                }

                var topology = options.Value.Topology;

                if (topology == null || !topology.Any())
                {
                    throw new InvalidOperationException("The RabbitMq topology is not defined.");
                }

                foreach (var key in topology.Keys)
                {
                    var exchangeTopology = topology[key];
                    await DeclareTopologyAsync(channel, exchangeTopology, cancellationToken);
                }

                _topologyInitialized = true;
                logger.LogInformation("RabbitMQ topologies (Exchanges and Queues) successfully initialized.");

            }
            catch (Exception ex)
            {
                _topologyInitialized = false;
                logger.LogError(ex, ex.Message);
                throw;
            }
            finally
            {
                _connectionLocker.Release();
            }
        }

        return channel;

    }

    public async Task<IConnection> GetConnectionAsync(CancellationToken cancellationToken = default)
    {
        if (_connection is { IsOpen: true })
        {
            return _connection;
        }

        await _connectionLocker.WaitAsync(cancellationToken);
        try
        {
            if (_connection is { IsOpen: true })
            {
                return _connection;
            }

            var factory = new ConnectionFactory
            {
                HostName = RabbitMqConfig.HostName,
                Port = RabbitMqConfig.Port,
                UserName = RabbitMqConfig.UserName,
                Password = RabbitMqConfig.Password,
                VirtualHost = RabbitMqConfig.VirtualHost
            };

            _connection = await factory.CreateConnectionAsync(cancellationToken);
            logger.LogInformation("RabbitMQ connection successfully established.");

            return _connection;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            throw;
        }
        finally
        {
            _connectionLocker.Release();
        }
    }

    #region private methods and functions

    private static async Task DeclareTopologyAsync(IChannel channel, ExchangeTopology topology, CancellationToken cancellationToken)
    {
        if (topology == null || string.IsNullOrWhiteSpace(topology.ExchangeName))
        {
            return;
        }

        // Declare exchange
        await channel.ExchangeDeclareAsync(
            exchange: topology.ExchangeName,
            type: topology.ExchangeType,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);

        if (!string.IsNullOrWhiteSpace(topology.QueueName))
        {
            // Declare queue
            await channel.QueueDeclareAsync(
                queue: topology.QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                cancellationToken: cancellationToken);

            if (topology.RoutingKeys.Any())
            {
                foreach (var routing_key in topology.RoutingKeys)
                {
                    // Bind queue to exchange
                    await channel.QueueBindAsync(
                        queue: topology.QueueName,
                        exchange: topology.ExchangeName,
                        routingKey: routing_key,
                        cancellationToken: cancellationToken);
                }
            }
        }
    }

    #endregion

    #region disposal functionality

    public async ValueTask DisposeAsync()
    {
        if (_connection != null)
        {
            await _connection.CloseAsync();
            _connection.Dispose();
        }

        _connectionLocker.Dispose();
    }

    public void Dispose()
    {
        DisposeAsync().AsTask().GetAwaiter().GetResult();
    }

    #endregion
}
