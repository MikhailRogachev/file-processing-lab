using infrastructure.Services.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace infrastructure.Extensions;

public static class ServicesInjection
{
    public static IServiceCollection InfrastructureServicesRegistration(this IServiceCollection services, ConfigurationManager configuration)
    {
        services.AddOptions<MessagingServiceSettings>().Bind(configuration.GetSection(nameof(MessagingServiceSettings)));
        services.AddScoped<IRabbitMqPublisher, RabbitMqPublisher>();

        return services;
    }
}
