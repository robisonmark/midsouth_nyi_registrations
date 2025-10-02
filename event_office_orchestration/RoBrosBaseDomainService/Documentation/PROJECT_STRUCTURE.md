# Project Structure

```
RoBrosBaseDomainService/
├── .github/
│   └── workflows/
│       └── build-and-publish.yml    # CI/CD pipeline
├── Data/
│   ├── JournalDbContext.cs          # EF Core DbContext
│   └── DesignTimeDbContextFactory.cs # Design-time factory for migrations
├── DTOs/
│   └── JournalDTOs.cs               # Data Transfer Objects
├── Examples/
│   └── ExampleIntegration.cs        # Integration examples
├── Extensions/
│   └── ServiceCollectionExtensions.cs # DI extensions
├── Migrations/
│   └── InitialCreate.cs             # EF Core migrations
├── Models/
│   └── EntityJournal.cs             # Entity model
├── Services/
│   ├── JournalService.cs            # Sync journal service
│   ├── AsyncJournalService.cs       # Async journal service
│   └── JournalBackgroundService.cs  # Background worker
├── Tests/
│   ├── RoBrosBaseDomainService.Tests.csproj
│   └── JournalServiceTests.cs       # Unit tests
├── .dockerignore                     # Docker ignore file
├── .env.example                      # Environment variables template
├── .gitignore                        # Git ignore file
├── appsettings.json                  # Application settings
├── appsettings.Development.json      # Development settings
├── CHANGELOG.md                      # Version history
├── docker-compose.yml                # Production Docker setup
├── docker-compose.dev.yml            # Development Docker setup
├── Dockerfile                        # Production Dockerfile
├── Dockerfile.dev                    # Development Dockerfile
├── GlobalUsings.cs                   # Global using directives
├── LICENSE                           # MIT License
├── Makefile                          # Development commands
├── nuget.config                      # NuGet configuration
├── QUICKSTART.md                     # Quick start guide
├── README.md                         # Main documentation
└── RoBrosBaseDomainService.csproj   # Project file
```

## Key Components

### Core Services

**JournalService** (`Services/JournalService.cs`)
- Synchronous journal operations
- Direct database access
- Used for queries and direct writes

**AsyncJournalService** (`Services/AsyncJournalService.cs`)
- Async journal operations via channels
- Non-blocking writes
- Recommended for API integration

**JournalBackgroundService** (`Services/JournalBackgroundService.cs`)
- Background worker that processes queued journal entries
- Runs in separate thread
- Handles retry logic

### Data Layer

**JournalDbContext** (`Data/JournalDbContext.cs`)
- EF Core context for journal tables
- Configures indexes and schema
- Supports PostgreSQL JSONB

**EntityJournal** (`Models/EntityJournal.cs`)
- Entity model for journal entries
- Maps to `entity_journals` table
- Contains audit fields and JSON payload

### Integration

**ServiceCollectionExtensions** (`Extensions/ServiceCollectionExtensions.cs`)
- `AddRoBrosJournalService()` method
- Configures all services and dependencies
- Database migration helper

### Configuration Files

**docker-compose.yml**
- Production setup
- PostgreSQL on port 5432
- Simple container setup

**docker-compose.dev.yml**
- Development setup with hot reload
- PostgreSQL on port 5433
- Volume mounts for live code editing
- API exposed on port 5000

**Dockerfile**
- Multi-stage build for production
- Creates NuGet package
- Optimized image size

**Dockerfile.dev**
- Development image with SDK
- Includes dotnet-ef tools
- Supports file watching

### Documentation

- **README.md**: Complete feature documentation
- **QUICKSTART.md**: Get started in minutes
- **PROJECT_STRUCTURE.md**: This file
- **CHANGELOG.md**: Version history
- **Examples/**: Integration code samples

### Development Tools

**Makefile**
- Common commands abstracted
- Development, build, and deploy tasks
- Database management

**.github/workflows/**
- Automated CI/CD
- NuGet package publishing
- Docker image building

## Database Schema

```sql
entity_journals
├── id (uuid, PK)
├── entity_id (varchar(255))
├── entity_type (varchar(255))
├── entity (jsonb)
├── created_by (varchar(255))
├── created_at (timestamp)
├── updated_by (varchar(255))
├── updated_at (timestamp)
└── version (integer)

Indexes:
- idx_entity_journals_entity_lookup (entity_id, entity_type, created_at)
- idx_entity_journals_created_at (created_at)
- idx_entity_journals_entity_version (entity_id, version)
```

## Design Patterns

1. **Repository Pattern**: JournalService abstracts data access
2. **Channel Pattern**: Async processing with bounded channels
3. **Background Service**: Long-running background worker
4. **Dependency Injection**: All services registered via DI
5. **Factory Pattern**: Design-time context factory for migrations

## Threading Model

```
Main Thread (API Request)
    ↓
IAsyncJournalService.EnqueueJournalEntryAsync()
    ↓
Channel.Writer.WriteAsync() [non-blocking]
    ↓
Return immediately to caller
    ↓
Background Thread (JournalBackgroundService)
    ↓
Channel.Reader.ReadAllAsync() [continuous loop]
    ↓
JournalService.CreateOrUpdateAsync() [scoped]
    ↓
Database write
```

## Integration Points

Your service integrates at these points:

1. **Program.cs**: Call `AddRoBrosJournalService()`
2. **Controllers/Services**: Inject `IAsyncJournalService`
3. **Database**: Share your connection string
4. **Migrations**: Run on startup with `EnsureJournalDatabaseCreatedAsync()`

## Extension Points

Future extension areas:

- Custom serialization providers
- Different database providers
- Event hooks for entity changes
- Custom retention policies
- Query optimization strategies
