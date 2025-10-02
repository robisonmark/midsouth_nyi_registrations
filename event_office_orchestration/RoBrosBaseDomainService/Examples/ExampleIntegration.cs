using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RoBrosBaseDomainService.DTOs;
using RoBrosBaseDomainService.Extensions;
using RoBrosBaseDomainService.Services;

namespace RoBrosBaseDomainService.Examples;

/// <summary>
/// Example integration of RoBrosBaseDomainService into your API
/// </summary>
public class ExampleIntegration
{
    /// <summary>
    /// Example Program.cs setup
    /// </summary>
    public static void ConfigureServices()
    {
        var builder = WebApplication.CreateBuilder();

        // Add your existing services
        builder.Services.AddControllers();
        
        // Add RoBros Journal Service
        // It will use YOUR database connection string
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        builder.Services.AddRoBrosJournalService(
            connectionString!, 
            channelCapacity: 1000
        );

        var app = builder.Build();

        // Ensure journal tables are created
        app.Services.EnsureJournalDatabaseCreatedAsync().Wait();

        app.MapControllers();
        app.Run();
    }
}

/// <summary>
/// Example controller showing async journaling
/// </summary>
public class ProductController
{
    private readonly IAsyncJournalService _journalService;

    public ProductController(IAsyncJournalService journalService)
    {
        _journalService = journalService;
    }

    public async Task<ProductResponse> CreateProduct(CreateProductRequest request, string userId)
    {
        // 1. Your business logic - create the product
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Price = request.Price,
            CreatedAt = DateTime.UtcNow
        };

        // Save to your database
        // await _yourDbContext.Products.AddAsync(product);
        // await _yourDbContext.SaveChangesAsync();

        // 2. Journal the entity (runs in background thread)
        var journalResponse = await _journalService.EnqueueJournalEntryAsync(
            new JournalRequest
            {
                EntityId = product.Id.ToString(),
                EntityType = nameof(Product),
                EntityPayload = product,
                UserId = userId
            });

        // 3. Return immediately with audit info
        return new ProductResponse
        {
            Product = product,
            CreatedBy = journalResponse.CreatedBy,
            CreatedAt = journalResponse.CreatedAt,
            UpdatedBy = journalResponse.UpdatedBy,
            UpdatedAt = journalResponse.UpdatedAt,
            Version = journalResponse.Version
        };
    }

    public async Task<ProductResponse> UpdateProduct(Guid id, UpdateProductRequest request, string userId)
    {
        // 1. Your business logic - update the product
        // var product = await _yourDbContext.Products.FindAsync(id);
        // product.Name = request.Name;
        // product.Price = request.Price;
        // await _yourDbContext.SaveChangesAsync();

        var product = new Product 
        { 
            Id = id, 
            Name = request.Name, 
            Price = request.Price 
        };

        // 2. Journal the update
        var journalResponse = await _journalService.EnqueueJournalEntryAsync(
            new JournalRequest
            {
                EntityId = id.ToString(),
                EntityType = nameof(Product),
                EntityPayload = product,
                UserId = userId
            });

        // 3. Return with updated audit info
        return new ProductResponse
        {
            Product = product,
            CreatedBy = journalResponse.CreatedBy,
            CreatedAt = journalResponse.CreatedAt,
            UpdatedBy = journalResponse.UpdatedBy,
            UpdatedAt = journalResponse.UpdatedAt,
            Version = journalResponse.Version
        };
    }
}

/// <summary>
/// Example service showing how to retrieve journal history
/// </summary>
public class AuditService
{
    private readonly IJournalService _journalService;

    public AuditService(IJournalService journalService)
    {
        _journalService = journalService;
    }

    public async Task<List<AuditEntry>> GetEntityHistory(string entityId, string entityType)
    {
        var history = await _journalService.GetHistoryAsync(
            new JournalHistoryRequest
            {
                EntityId = entityId,
                EntityType = entityType,
                Limit = 50 // Optional: limit results
            });

        return history.Select(h => new AuditEntry
        {
            Id = h.Id,
            EntityData = h.Entity, // JSON string
            ModifiedBy = h.UpdatedBy,
            ModifiedAt = h.UpdatedAt,
            Version = h.Version
        }).ToList();
    }

    public async Task<AuditEntry?> GetLatestVersion(string entityId, string entityType)
    {
        var latest = await _journalService.GetLatestAsync(entityId, entityType);
        
        if (latest == null) return null;

        return new AuditEntry
        {
            Id = latest.Id,
            EntityData = latest.Entity,
            ModifiedBy = latest.UpdatedBy,
            ModifiedAt = latest.UpdatedAt,
            Version = latest.Version
        };
    }
}

// Example DTOs
public record Product
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public DateTime CreatedAt { get; set; }
}

public record CreateProductRequest(string Name, decimal Price);
public record UpdateProductRequest(string Name, decimal Price);

public record ProductResponse
{
    public required Product Product { get; init; }
    public required string CreatedBy { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required string UpdatedBy { get; init; }
    public required DateTime UpdatedAt { get; init; }
    public required int Version { get; init; }
}

public record AuditEntry
{
    public required Guid Id { get; init; }
    public required string EntityData { get; init; }
    public required string ModifiedBy { get; init; }
    public required DateTime ModifiedAt { get; init; }
    public required int Version { get; init; }
}