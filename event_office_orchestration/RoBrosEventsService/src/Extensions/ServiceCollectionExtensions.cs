using System.Data;

using RoBrosEventsService.Data;
using RoBrosEventsService.Interfaces;
using RoBrosEventsService.Services;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace RoBrosEventsService.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEventPackage(
        this IServiceCollection services,
        NpgsqlDataSource dataSource)
    {
       
        services.AddScoped<ISqlProvider, PostgresProvider>();

        // Register repository
        services.AddScoped<IEventRepository>(provider =>
        {
            var sqlProvider = provider.GetRequiredService<ISqlProvider>();
            var logger = provider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<EventRepository>>();

            // TODO: I think logger should come from the consuming service and default if not provided
            return new EventRepository(dataSource, sqlProvider, logger);
        });

        // Register service
        services.AddScoped<IEventService, EventService>();

        return services;
    }

    // public static IServiceCollection AddAddressPackage<TSqlProvider>(
    //     this IServiceCollection services,
    //     string connectionString)
    //     where TSqlProvider : class, ISqlProvider
    // {
    //     return services.AddAddressPackage(connectionString, typeof(TSqlProvider));
    // }

    // public static IServiceCollection AddAddressPackage<TSqlProvider>(
    //     this IServiceCollection services,
    //     NpgsqlDataSource dataSource)
    //     where TSqlProvider : class, ISqlProvider
    // {
    //     return services.AddAddressPackage(dataSource, typeof(TSqlProvider));
    // }

   
}