using aws_agent.Services;
using Microsoft.Extensions.Configuration;

namespace aws_agent.Extensions;

public static class AwsServicesInjectionExtensions
{
    /// <summary>
    /// Registers AWS options, message consumers, Amazon SQS clients, and background listener services into the dependency injection container.
    /// </summary>
    /// <remarks>
    /// This extension method configures the following components:
    /// <list type="bullet">
    ///   <item>
    ///     <description>Binds application configuration settings to the <see cref="AwsOptions"/> strongly-typed options object.</description>
    ///   </item>
    ///   <item>
    ///     <description>Registers <see cref="SqsMessageConsumer"/> as a scoped service implementation for <see cref="IMessageConsumer{T}"/>.</description>
    ///   </item>
    ///   <item>
    ///     <description>Registers the official AWS SDK <see cref="Amazon.SQS.IAmazonSQS"/> client as a singleton service via the <c>AWSSDK.Extensions.NETCore.Setup</c> package.</description>
    ///   </item>
    ///   <item>
    ///     <description>Registers <see cref="SqsListener"/> as a hosted background service to handle long-running SQS queue consumption.</description>
    ///   </item>
    /// </list>
    /// </remarks>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <param name="configurationManager">The <see cref="ConfigurationManager"/> instance used to bind configuration sections.</param>
    /// <returns>
    /// The same <see cref="IServiceCollection"/> instance so that additional calls can be chained.
    /// </returns>
    public static IServiceCollection AwsServicesInjection(this IServiceCollection services, ConfigurationManager configurationManager)
    {
        services.AddOptions<AwsOptions>().Bind(configurationManager.GetSection(nameof(AwsOptions)));
        services.AddScoped<IMessageConsumer<AmazonSQSClient>, SqsMessageConsumer>();
        services.AddSingleton<IAwsClientConnectionFactory, AwsClientConnectionfactory>();

        // Background services
        services.AddHostedService<SqsListener>();
        return services;
    }
}