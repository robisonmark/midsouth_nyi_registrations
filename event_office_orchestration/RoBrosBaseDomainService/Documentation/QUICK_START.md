# Quick Start Guide

Get up and running with RoBrosBaseDomainService in minutes.

## 1. Development Setup (Hot Reload)

```bash
# Clone the repository
git clone <your-repo-url>
cd RoBrosBaseDomainService

# Start development environment with hot reload
make dev-up

# Or using docker-compose directly
docker-compose -f docker-compose.dev.yml up
```

The service will start with:
- Hot reload enabled (changes auto-reload)
- PostgreSQL on port 5433
- Development API on port 5000

### Making Changes

Just edit any `.cs` file and save - the service will automatically reload!

```bash
# Watch the logs
make dev-logs

# Stop when done
make dev-down
```

## 2. Integration into Your Service

### Step 1: Add the Package

```bash
# If published to NuGet
dotnet add package RoBrosBaseDomainService

# Or reference locally
dotnet add reference ../RoBrosBaseDomainService/RoBrosBaseDomainService.csproj
```

### Step 2: Configure in Program.cs

```csharp
using RoBrosBaseDomainService.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add your services
builder.Services.AddControllers();

// Add RoBros Journal Service - uses YOUR database
builder.Services.AddRoBrosJournalService(
    builder.Configuration.GetConnectionString("DefaultConnection")!
);

var app = builder.Build();

// Create journal tables in your database
await app.Services.EnsureJournalDatabaseCreatedAsync();

app.MapControllers();
app.Run();
```

### Step 3: Use in Your Code

```csharp
using RoBrosBaseDomainService.Services;
using RoBrosBaseDomainService.DTOs;

public class MyController : ControllerBase
{
    private readonly IAsyncJournalService _journal;

    public MyController(IAsyncJournalService journal)
    {
        _journal = journal;
    }

    [HttpPost("products")]
    public async Task<IActionResult> CreateProduct(ProductDto dto)
    {
        // Your business logic
        var product = new Product { Id = Guid.NewGuid(), Name = dto.Name };
        
        // Journal it (async, non-blocking)
        var audit = await _journal.EnqueueJournalEntryAsync(new JournalRequest
        {
            EntityId = product.Id.ToString(),
            EntityType = "Product",
            EntityPayload = product,
            UserId = User.Identity?.Name ?? "system"
        });

        return Ok(new { product, audit });
    }
}
```

## 3. Common Commands

```bash
# Development
make dev-up          # Start dev environment
make dev-down        # Stop dev environment
make dev-logs        # View logs

# Building
make build           # Build the project
make pack            # Create NuGet package
make docker-build    # Build Docker image

# Database
make migrate         # Run migrations
make migration-add NAME=MyMigration  # Add new migration
make db-connect      # Connect to PostgreSQL
make db-reset        # Reset database (WARNING: destroys data)

# Production
make docker-up       # Start production containers
make docker-down     # Stop production containers
```

## 4. Database Connection

### Development
```
Host=localhost;Port=5433;Database=robros_journal_db;Username=robros_user;Password=robros_password
```

### Your Service (Production)
Use your existing connection string - the journal will create its own table:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=mydb.example.com;Database=myapp;Username=myuser;Password=mypass"
  }
}
```

## 5. Viewing Journal Entries

```bash
# Connect to database
make db-connect

# Query journal entries
SELECT 
    entity_id,
    entity_type,
    created_by,
    created_at,
    updated_by,
    updated_at,
    version
FROM entity_journals
ORDER BY created_at DESC
LIMIT 10;

# View JSON payload
SELECT entity_id, entity::json FROM entity_journals LIMIT 1;
```

## 6. Retrieving History in Code

```csharp
// Inject IJournalService (not IAsyncJournalService)
private readonly IJournalService _journal;

// Get latest version
var latest = await _journal.GetLatestAsync("product-123", "Product");

// Get full history
var history = await _journal.GetHistoryAsync(new JournalHistoryRequest
{
    EntityId = "product-123",
    EntityType = "Product",
    Limit = 10
});

// Deserialize entity
var product = JsonSerializer.Deserialize<Product>(history[0].Entity);
```

## 7. Performance Tips

1. **Use Async Service**: Always use `IAsyncJournalService` for writes (non-blocking)
2. **Adjust Channel Capacity**: Increase for high-load scenarios
   ```csharp
   services.AddRoBrosJournalService(connStr, channelCapacity: 5000);
   ```
3. **Limit History Queries**: Use the `Limit` parameter
4. **Index Strategy**: The service creates indexes automatically

## 8. Troubleshooting

### Service Won't Start
```bash
# Check if ports are in use
lsof -i :5433  # PostgreSQL
lsof -i :5000  # API

# View logs
docker-compose -f docker-compose.dev.yml logs
```

### Hot Reload Not Working
```bash
# Restart container
docker-compose -f docker-compose.dev.yml restart robros-service-dev

# Check file watcher is enabled
docker-compose -f docker-compose.dev.yml exec robros-service-dev \
    printenv DOTNET_USE_POLLING_FILE_WATCHER
# Should output: true
```

### Database Connection Issues
```bash
# Verify PostgreSQL is healthy
docker-compose ps

# Test connection
docker exec robros-journal-postgres-dev \
    pg_isready -U robros_user
```

## Next Steps

- Read the full [README.md](README.md) for detailed documentation
- Check [Examples/ExampleIntegration.cs](Examples/ExampleIntegration.cs) for more patterns
- Review the [API Documentation](#) (if available)

## Support

For issues, questions, or contributions:
- Open an issue on GitHub
- Check existing documentation
- Review example code