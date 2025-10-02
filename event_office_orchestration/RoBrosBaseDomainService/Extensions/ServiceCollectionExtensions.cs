using System.Threading.Channels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RoBrosBaseDomainService.Data;
using RoBrosBaseDomainService.Services;

namespace RoBrosBaseDomainService.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRoBrosJournalService(
        this IServiceCollection services,
        string connectionString,
        int channelCapacity = 1000)
    {
        // Add DbContext
        services.AddDbContext<JournalDbContext>(options =>
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorCodesToAdd: null);
            }));

        // Add Channel for background processing
        services.AddSingleton(Channel.CreateBounded<JournalWorkItem>(new BoundedChannelOptions(channelCapacity)
        {
            FullMode = BoundedChannelFullMode.Wait
        }));

        // Add services
        services.AddScoped<IJournalService, JournalService>();
        services.AddSingleton<IAsyncJournalService, AsyncJournalService>();
        services.AddHostedService<JournalBackgroundService>();

        return services;
    }

    public static async Task EnsureJournalDatabaseCreatedAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<JournalDbContext>();
        await context.Database.MigrateAsync();
    }
}