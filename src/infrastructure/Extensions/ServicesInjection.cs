using domain.Events.FileEvents;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace infrastructure.Extensions;

public static class ServicesInjection
{
    public static IServiceCollection InfrastructureServicesRegistration(this IServiceCollection services, ConfigurationManager configuration)
    {
        services.AddOptions<RabbitMqMessagingConfig>().Bind(configuration.GetSection(nameof(RabbitMqMessagingConfig)));

        services.AddSingleton<IRabbitMqConnectionFactory, RabbitMqConnectionFactory>();
        services.AddScoped<IRabbitMqPublisher, RabbitMqPublisher>();
        services.AddSingleton<IRabbitMqMessageDeserializer, MessageDeserializer>();

        // eventHandles registration
        services.AddKeyedScoped<IEventHandler<FileUpdatedEvent>, FileChangedEventHandler>(RoutingKeys.AWS_FILECHANGED);
        services.AddKeyedScoped<IEventHandler<FileDeletedEvent>, FileDeletedEventHandler>(RoutingKeys.AWS_FILEDELETED);

        // add background service
        services.AddHostedService<RabbitMqListener>();

        return services;
    }
}
