using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using RoBrosBaseDomainService.DTOs;

namespace RoBrosBaseDomainService.Services;

public interface IAsyncJournalService
{
    Task<JournalResponse> EnqueueJournalEntryAsync(JournalRequest request, CancellationToken cancellationToken = default);
}

public class AsyncJournalService : IAsyncJournalService
{
    private readonly Channel<JournalWorkItem> _channel;
    private readonly ILogger<AsyncJournalService> _logger;

    public AsyncJournalService(Channel<JournalWorkItem> channel, ILogger<AsyncJournalService> logger)
    {
        _channel = channel;
        _logger = logger;
    }

    public async Task<JournalResponse> EnqueueJournalEntryAsync(JournalRequest request, CancellationToken cancellationToken = default)
    {
        var completionSource = new TaskCompletionSource<JournalResponse>();
        var workItem = new JournalWorkItem
        {
            Request = request,
            CompletionSource = completionSource
        };

        await _channel.Writer.WriteAsync(workItem, cancellationToken);
        _logger.LogDebug("Journal entry queued for EntityId={EntityId}", request.EntityId);

        return await completionSource.Task;
    }
}