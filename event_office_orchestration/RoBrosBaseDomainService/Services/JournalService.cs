using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RoBrosBaseDomainService.Data;
using RoBrosBaseDomainService.DTOs;
using RoBrosBaseDomainService.Models;

namespace RoBrosBaseDomainService.Services;

public interface IJournalService
{
    Task<JournalResponse> CreateOrUpdateAsync(JournalRequest request, CancellationToken cancellationToken = default);
    Task<JournalHistoryResponse?> GetLatestAsync(string entityId, string entityType, CancellationToken cancellationToken = default);
    Task<List<JournalHistoryResponse>> GetHistoryAsync(JournalHistoryRequest request, CancellationToken cancellationToken = default);
    Task<JournalHistoryResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}

public class JournalService : IJournalService
{
    private readonly JournalDbContext _context;
    private readonly ILogger<JournalService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public JournalService(JournalDbContext context, ILogger<JournalService> logger)
    {
        _context = context;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    public async Task<JournalResponse> CreateOrUpdateAsync(JournalRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var now = DateTime.UtcNow;
            var entityJson = JsonSerializer.Serialize(request.EntityPayload, _jsonOptions);

            var existingEntry = await _context.EntityJournals
                .Where(e => e.EntityId == request.EntityId && e.EntityType == request.EntityType)
                .OrderByDescending(e => e.Version)
                .FirstOrDefaultAsync(cancellationToken);

            var journal = new EntityJournal
            {
                Id = Guid.NewGuid(),
                EntityId = request.EntityId,
                EntityType = request.EntityType,
                Entity = entityJson,
                CreatedBy = existingEntry?.CreatedBy ?? request.UserId,
                CreatedAt = existingEntry?.CreatedAt ?? now,
                UpdatedBy = request.UserId,
                UpdatedAt = now,
                Version = (existingEntry?.Version ?? 0) + 1
            };

            _context.EntityJournals.Add(journal);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Journal entry created: EntityId={EntityId}, EntityType={EntityType}, Version={Version}",
                journal.EntityId, journal.EntityType, journal.Version);

            return new JournalResponse
            {
                Id = journal.Id,
                EntityId = journal.EntityId,
                CreatedBy = journal.CreatedBy,
                CreatedAt = journal.CreatedAt,
                UpdatedBy = journal.UpdatedBy,
                UpdatedAt = journal.UpdatedAt,
                Version = journal.Version
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating journal entry for EntityId={EntityId}", request.EntityId);
            throw;
        }
    }

    public async Task<JournalHistoryResponse?> GetLatestAsync(string entityId, string entityType, CancellationToken cancellationToken = default)
    {
        var latest = await _context.EntityJournals
            .Where(e => e.EntityId == entityId && e.EntityType == entityType)
            .OrderByDescending(e => e.Version)
            .FirstOrDefaultAsync(cancellationToken);

        return latest == null ? null : MapToHistoryResponse(latest);
    }

    public async Task<List<JournalHistoryResponse>> GetHistoryAsync(JournalHistoryRequest request, CancellationToken cancellationToken = default)
    {
        var query = _context.EntityJournals
            .Where(e => e.EntityId == request.EntityId && e.EntityType == request.EntityType)
            .OrderByDescending(e => e.Version);

        if (request.Limit.HasValue)
        {
            query = (IOrderedQueryable<EntityJournal>)query.Take(request.Limit.Value);
        }

        var results = await query.ToListAsync(cancellationToken);
        return results.Select(MapToHistoryResponse).ToList();
    }

    public async Task<JournalHistoryResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var journal = await _context.EntityJournals
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        return journal == null ? null : MapToHistoryResponse(journal);
    }

    private static JournalHistoryResponse MapToHistoryResponse(EntityJournal journal)
    {
        return new JournalHistoryResponse
        {
            Id = journal.Id,
            EntityId = journal.EntityId,
            EntityType = journal.EntityType,
            Entity = journal.Entity,
            CreatedBy = journal.CreatedBy,
            CreatedAt = journal.CreatedAt,
            UpdatedBy = journal.UpdatedBy,
            UpdatedAt = journal.UpdatedAt,
            Version = journal.Version
        };
    }
}