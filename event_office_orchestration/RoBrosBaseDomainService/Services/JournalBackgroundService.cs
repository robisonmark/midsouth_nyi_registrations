using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RoBrosBaseDomainService.DTOs;

namespace RoBrosBaseDomainService.Services;

public class JournalBackgroundService : BackgroundService
{
    private readonly Channel<JournalWorkItem> _channel;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<JournalBackgroundService> _logger;

    public JournalBackgroundService(
        Channel<JournalWorkItem> channel,
        IServiceProvider serviceProvider,
        ILogger<JournalBackgroundService> logger)
    {
        _channel = channel;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Journal Background Service is starting");

        await foreach (var workItem in _channel.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var journalService = scope.ServiceProvider.GetRequiredService<IJournalService>();

                var response = await journalService.CreateOrUpdateAsync(workItem.Request, stoppingToken);
                workItem.CompletionSource.SetResult(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing journal work item");
                workItem.CompletionSource.SetException(ex);
            }
        }

        _logger.LogInformation("Journal Background Service is stopping");
    }
}

public record JournalWorkItem
{
    public required JournalRequest Request { get; init; }
    public required TaskCompletionSource<JournalResponse> CompletionSource { get; init; }
}