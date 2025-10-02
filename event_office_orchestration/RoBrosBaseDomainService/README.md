# RoBrosBaseDomainService
RoBrosBaseDomainService is foundational domain in the Momentum Office App.  It allows for setting up the base fields required for all entities.  It could also set up basic database crud operations, allowing entities to be focused on only the entity itself consuming this service.  This would allow for consistency in how the EntityServices write to the Database.

# RoBrosBaseDomainService

A self-contained .NET 9.0 journal/audit service library that provides entity tracking with async processing capabilities. This library can be integrated into any API service and will create its own journal table in the calling service's database.

## Features

- **Self-contained library**: Can be integrated into any .NET service
- **Async processing**: Background thread processing using Channels for high performance
- **Database agnostic integration**: Uses the calling service's database connection
- **Full audit trail**: Tracks created_by, created_at, updated_by, updated_at
- **Versioning**: Maintains version history for all entities
- **JSONB storage**: Stores entity payloads as JSON for flexibility
- **PostgreSQL optimized**: Uses JSONB type and appropriate indexes

## Architecture

- **Channel-based processing**: Uses bounded channels for thread-safe async operations
- **Background service**: Dedicated background service processes journal entries
- **Scoped DbContext**: Proper context lifecycle management
- **Immediate response**: Returns audit metadata immediately while processing in background

## Installation

### As a NuGet Package

```bash
dotnet add package RoBrosBaseDomainService
```

### Integration in Your Service

```csharp
// In Program.cs or Startup.cs
using RoBrosBaseDomainService.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add the journal service with your database connection string
builder.Services.AddRoBrosJournalService(
    builder.Configuration.GetConnectionString("DefaultConnection"),
    channelCapacity: 1000  // Optional: defaults to 1000
);

var app = builder.Build();

// Ensure database migrations are applied
await app.Services.EnsureJournalDatabaseCreatedAsync();

app.Run();
```

## Usage

### Async Journal Entry (Recommended)

```csharp
public class YourController : ControllerBase
{
    private readonly IAsyncJournalService _journalService;

    public YourController(IAsyncJournalService journalService)
    {
        _journalService = journalService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateEntity(YourEntityDto dto)
    {
        // Your business logic here
        var entity = CreateYourEntity(dto);
        
        // Journal the entity (runs in background thread)
        var journalResponse = await _journalService.EnqueueJournalEntryAsync(
            new JournalRequest
            {
                EntityId = entity.Id.ToString(),
                EntityType = "YourEntity",
                EntityPayload = entity,
                UserId = User.Identity?.Name ?? "system"
            });

        return Ok(new
        {
            Entity = entity,
            Audit = journalResponse
        });
    }
}
```

### Synchronous Journal Entry

```csharp
public class YourService
{
    private readonly IJournalService _journalService;

    public YourService(IJournalService journalService)
    {
        _journalService = journalService;
    }

    public async Task ProcessEntity(string entityId)
    {
        // Direct synchronous journaling
        var response = await _journalService.CreateOrUpdateAsync(
            new JournalRequest
            {
                EntityId = entityId,
                EntityType = "Product",
                EntityPayload = new { Name = "Product Name", Price = 99.99 },
                UserId = "user123"
            });
    }
}
```

### Retrieving Journal History

```csharp
// Get latest version
var latest = await _journalService.GetLatestAsync("entity-123", "Product");

// Get full history
var history = await _journalService.GetHistoryAsync(
    new JournalHistoryRequest
    {
        EntityId = "entity-123",
        EntityType = "Product",
        Limit = 10  // Optional
    });

// Get specific version by ID
var version = await _journalService.GetByIdAsync(journalId);
```

## Development

### Prerequisites

- .NET 9.0 SDK
- Docker and Docker Compose

### Local Development with Hot Reload

```bash
# Start development environment with hot reload
docker-compose -f docker-compose.dev.yml up

# The service will automatically reload when you make changes
# PostgreSQL will be available on localhost:5433
```

### Running Migrations

```bash
# Add a new migration
dotnet ef migrations add MigrationName --project RoBrosBaseDomainService.csproj

# Apply migrations
dotnet ef database update --project RoBrosBaseDomainService.csproj
```

### Building the Package

```bash
# Build the library
dotnet build

# Create NuGet package
dotnet pack -c Release

# Build Docker image
docker build -t robros-journal-service:1.0.0 .
```

### Testing

```bash
# Run with docker-compose
docker-compose up

# Connect to PostgreSQL
docker exec -it robros-journal-postgres psql -U robros_user -d robros_journal_db

# View journal entries
SELECT * FROM entity_journals ORDER BY created_at DESC;
```

## Database Schema

```sql
CREATE TABLE entity_journals (
    id UUID PRIMARY KEY,
    entity_id VARCHAR(255) NOT NULL,
    entity_type VARCHAR(255) NOT NULL,
    entity JSONB NOT NULL,
    created_by VARCHAR(255) NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL,
    updated_by VARCHAR(255) NOT NULL,
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL,
    version INTEGER NOT NULL
);

-- Indexes
CREATE INDEX idx_entity_journals_entity_lookup 
    ON entity_journals(entity_id, entity_type, created_at);
    
CREATE INDEX idx_entity_journals_created_at 
    ON entity_journals(created_at);
    
CREATE INDEX idx_entity_journals_entity_version 
    ON entity_journals(entity_id, version);
```

## Configuration

### Connection String

The service uses the connection string provided during registration:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=myapp_db;Username=user;Password=pass"
  }
}
```

### Channel Capacity

Adjust the channel capacity based on your load:

```csharp
services.AddRoBrosJournalService(connectionString, channelCapacity: 5000);
```

## Performance Considerations

- **Background Processing**: Journal entries are processed in a separate thread, preventing blocking of main API requests
- **Bounded Channel**: Configurable capacity prevents memory issues under high load
- **Connection Pooling**: EF Core connection pooling is enabled by default
- **Retry Logic**: Built-in retry mechanism for transient database failures
- **Indexes**: Optimized indexes for common query patterns

## Versioning

The library follows semantic versioning. Current version: **1.0.0**

To create a new version:

```bash
# Update version in .csproj
dotnet pack -c Release /p:Version=1.1.0

# Build new Docker image
docker build -t robros-journal-service:1.1.0 .
```

## Integration Examples

### With Multiple Entities

```csharp
public class OrderService
{
    private readonly IAsyncJournalService _journal;

    public async Task<Order> CreateOrder(CreateOrderDto dto)
    {
        var order = new Order { /* ... */ };
        await _dbContext.Orders.AddAsync(order);
        await _dbContext.SaveChangesAsync();

        // Journal the order
        await _journal.EnqueueJournalEntryAsync(new JournalRequest
        {
            EntityId = order.Id.ToString(),
            EntityType = "Order",
            EntityPayload = order,
            UserId = dto.UserId
        });

        return order;
    }

    public async Task<Order> UpdateOrder(Guid id, UpdateOrderDto dto)
    {
        var order = await _dbContext.Orders.FindAsync(id);
        // Update order properties
        await _dbContext.SaveChangesAsync();

        // Journal the update
        await _journal.EnqueueJournalEntryAsync(new JournalRequest
        {
            EntityId = order.Id.ToString(),
            EntityType = "Order",
            EntityPayload = order,
            UserId = dto.UserId
        });

        return order;
    }
}
```

### Querying by Entity ID from Multiple Services

```csharp
// Service A journals a customer
await _journal.EnqueueJournalEntryAsync(new JournalRequest
{
    EntityId = "customer-123",
    EntityType = "Customer",
    EntityPayload = customer,
    UserId = "user1"
});

// Service B can retrieve the journal by entity_id
var customerHistory = await _journalService.GetHistoryAsync(
    new JournalHistoryRequest
    {
        EntityId = "customer-123",
        EntityType = "Customer"
    });
```

## Troubleshooting

### Database Connection Issues

```bash
# Check if PostgreSQL is running
docker ps | grep postgres

# View logs
docker-compose logs postgres
```

### Migration Issues

```bash
# Reset database (development only)
docker-compose down -v
docker-compose up -d
```

### Hot Reload Not Working

```bash
# Ensure DOTNET_USE_POLLING_FILE_WATCHER is set
# Restart the development container
docker-compose -f docker-compose.dev.yml restart robros-service-dev
```

## License

MIT License - See LICENSE file for details

## Contributing

Contributions are welcome! Please open an issue or submit a pull request.