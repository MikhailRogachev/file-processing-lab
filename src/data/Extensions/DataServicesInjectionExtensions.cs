using data.Context;
using data.Repositories;
using domain.Interfaces.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace data.Extensions;

public static class DataServicesInjectionExtensions
{
    public static IServiceCollection DataServicesInjection(this IServiceCollection services, ConfigurationManager configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // repositories
        services.AddScoped<IMediaPackageRepository, MediaPackageRepository>();

        return services;
    }
}
