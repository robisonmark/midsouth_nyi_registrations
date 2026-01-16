using System.Data;
using RoBrosEventsService.Data;
using RoBrosEventsService.Interfaces;
using RoBrosEventsService.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Data.SqlClient;

namespace RoBrosEventsService.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEventPackage(
        this IServiceCollection services,
        string connectionString,
        Type? customSqlProviderType = null)
    {
        // Register SQL provider (default or custom)
        if (customSqlProviderType != null && typeof(ISqlProvider).IsAssignableFrom(customSqlProviderType))
        {
            services.AddScoped(typeof(ISqlProvider), customSqlProviderType);
        }
        else
        {
            services.AddScoped<ISqlProvider, SqlServerProvider>();
        }

        // Register repository
        services.AddScoped<IEventRepository>(provider =>
        {
            var sqlProvider = provider.GetRequiredService<ISqlProvider>();
            var logger = provider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<EventRepository>>();
            return new EventRepository(connectionString, sqlProvider, logger);
        });

        // Register service (both interface and concrete class)
        services.AddScoped<IEventService, EventService>();
        services.AddScoped<EventService>();

        return services;
    }

    public static IServiceCollection AddEventPackage<TSqlProvider>(
        this IServiceCollection services,
        string connectionString)
        where TSqlProvider : class, ISqlProvider
    {
        return services.AddEventPackage(connectionString, typeof(TSqlProvider));
    }
}