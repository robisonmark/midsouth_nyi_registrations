using System.Data;

using EventOfficeApi.RoBrosAddressesService.Data;
using EventOfficeApi.RoBrosAddressesService.Interfaces;
using EventOfficeApi.RoBrosAddressesService.Services;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace EventOfficeApi.RoBrosAddressesService.Extensions;

public static class ServiceCollectionExtensions
{
    // public static IServiceCollection AddAddressService(
    //     this IServiceCollection services,
    //     Action<AddressServiceOptions>? configureOptions = null)
    // {
    //     var options = new AddressServiceOptions();
    //     configureOptions?.Invoke(options);
    // 
    // 
    // NOTE: HANGING ON BECAUSE OF SINGLETON
    // 
    // 
    //     services.AddSingleton(options);
    //     services.AddScoped<IAddressService, Services.AddressService>();

    //     return services;
    // }

    // public static IServiceCollection AddAddressService(
    //     this IServiceCollection services,
    //     Func<IServiceProvider, IDbConnection> connectionFactory,
    //     Action<AddressServiceOptions>? configureOptions = null)
    // {
    //     services.AddAddressService(configureOptions);
    //     services.AddScoped(connectionFactory);

    //     return services;
    // }

    //New From Claude 
    // public static IServiceCollection AddAddressPackage(
    //    this IServiceCollection services,
    //    string connectionString,
    //    Type? customSqlProviderType = null)
    // {
    //     // Register SQL provider (default or custom)
    //     if (customSqlProviderType != null && typeof(ISqlProvider).IsAssignableFrom(customSqlProviderType))
    //     {
    //         services.AddScoped(typeof(ISqlProvider), customSqlProviderType);
    //     }
    //     else
    //     {
    //         services.AddScoped<ISqlProvider, PostgreSqlProvider>();
    //     }

    //     // Register repository
    //     services.AddScoped<IAddressRepository>(provider =>
    //     {
    //         var sqlProvider = provider.GetRequiredService<ISqlProvider>();
    //         var logger = provider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<AddressRepository>>();
    //         return new AddressRepository(connectionString, sqlProvider, logger);
    //         // this needs to take the data source instead of connection string
    //     });

    //     // Register service
    //     services.AddScoped<IAddressService, AddressService>();

    //     return services;
    // }

    public static IServiceCollection AddAddressPackage(
        this IServiceCollection services,
        NpgsqlDataSource dataSource,
        Type? customSqlProviderType = null)
    {
        // Register SQL provider (default or custom)
        if (customSqlProviderType != null && typeof(ISqlProvider).IsAssignableFrom(customSqlProviderType))
        {
            services.AddScoped(typeof(ISqlProvider), customSqlProviderType);
        }
        else
        {
            services.AddScoped<ISqlProvider, PostgreSqlProvider>();
        }

        // Register repository
        services.AddScoped<IAddressRepository>(provider =>
        {
            var sqlProvider = provider.GetRequiredService<ISqlProvider>();
            var logger = provider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<AddressRepository>>();

            // TODO: I think logger should come from the consuming service and default if not provided
            return new AddressRepository(dataSource, sqlProvider, logger);
        });

        // Register service
        services.AddScoped<IAddressService, AddressService>();

        return services;
    }

    // public static IServiceCollection AddAddressPackage<TSqlProvider>(
    //     this IServiceCollection services,
    //     string connectionString)
    //     where TSqlProvider : class, ISqlProvider
    // {
    //     return services.AddAddressPackage(connectionString, typeof(TSqlProvider));
    // }

    public static IServiceCollection AddAddressPackage<TSqlProvider>(
        this IServiceCollection services,
        NpgsqlDataSource dataSource)
        where TSqlProvider : class, ISqlProvider
    {
        return services.AddAddressPackage(dataSource, typeof(TSqlProvider));
    }

   
}