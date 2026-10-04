using domain.Interfaces.Outbox;

namespace infrastructure.Services.OutboxService;

public class OutboxEventPublisher(
    ILogger<OutboxEventPublisher> logger,
    AppDbContext context
    ) : IOutboxEventPublisher
{
    private Dictionary<string, Type> _types = new Dictionary<string, Type>();


    public Task RunAsync()
    {
        throw new NotImplementedException();
    }
}
