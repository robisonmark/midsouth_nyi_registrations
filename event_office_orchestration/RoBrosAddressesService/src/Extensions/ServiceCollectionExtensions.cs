using System.Data;

using EventOfficeApi.AddressService.Data;
using EventOfficeApi.AddressService.Interfaces;
using EventOfficeApi.AddressService.Services;
using Microsoft.Extensions.DependencyInjection;

namespace EventOfficeApi.AddressService.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAddressService(
        this IServiceCollection services,
        Action<AddressServiceOptions>? configureOptions = null)
    {
        var options = new AddressServiceOptions();
        configureOptions?.Invoke(options);

        services.AddSingleton(options);
        services.AddScoped<IAddressService, Services.AddressService>();

        return services;
    }

    public static IServiceCollection AddAddressService(
        this IServiceCollection services,
        Func<IServiceProvider, IDbConnection> connectionFactory,
        Action<AddressServiceOptions>? configureOptions = null)
    {
        services.AddAddressService(configureOptions);
        services.AddScoped(connectionFactory);

        return services;
    }

    //New From Claude 
    public static IServiceCollection AddAddressPackage(
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
            services.AddScoped<ISqlProvider, PostgreSqlProvider>();
        }

        // Register repository
        services.AddScoped<IAddressRepository>(provider =>
        {
            var sqlProvider = provider.GetRequiredService<ISqlProvider>();
            var logger = provider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<AddressRepository>>();
            return new AddressRepository(connectionString, sqlProvider, logger);
        });

        // Register service
        services.AddScoped<IAddressService, AddressService>();

        return services;
    }

    public static IServiceCollection AddAddressPackage<TSqlProvider>(
        this IServiceCollection services,
        string connectionString)
        where TSqlProvider : class, ISqlProvider
    {
        return services.AddAddressPackage(connectionString, typeof(TSqlProvider));
    }
}