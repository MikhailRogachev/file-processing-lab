using domain.Interfaces.Outbox;
using infrastructure.Services.OutboxService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace infrastructure.Extensions;

public static class ServicesInjection
{
    public static IServiceCollection InfrastructureServicesRegistration(this IServiceCollection services, ConfigurationManager configuration)
    {
        services.AddOptions<RabbitMqMessagingConfig>().Bind(configuration.GetSection(nameof(RabbitMqMessagingConfig)));
        services.AddOptions<OutboxMessageOptions>().Bind(configuration.GetSection(nameof(OutboxMessageOptions)));

        services.AddSingleton<IRabbitMqConnectionFactory, RabbitMqConnectionFactory>();
        services.AddSingleton<IRabbitMqPublisher, RabbitMqPublisher>();
        services.AddSingleton<IRabbitMqMessageDeserializer, MessageDeserializer>();

        services.AddScoped<IOutboxEventPublisher, OutboxEventPublisher>();


        // eventHandles registration
        services.AddScoped<IEventHandler<FileUpdatedEvent>, FileChangedEventHandler>();
        services.AddScoped<IEventHandler<FileDeletedEvent>, FileDeletedEventHandler>();
        services.AddScoped<IEventHandler<ValidateMediaAssetEvent>, FileValidateEventHandler>();

        // add background service
        services.AddHostedService<RabbitMqListener>();
        services.AddHostedService<OutboxEventService>();

        return services;
    }
}
