using Microsoft.Extensions.DependencyInjection;
using System.Data;
using YourCompany.AddressService.Configuration;
using YourCompany.AddressService.Services;

namespace YourCompany.AddressService.Extensions;

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
}