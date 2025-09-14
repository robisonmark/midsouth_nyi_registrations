# AddressPackage

A .NET 9.0 NuGet package for managing physical addresses with PostgreSQL support. This package provides a complete solution for address management that can be consumed by other services while maintaining independence and extensibility.

## Features

- 🏠 Complete address management (CRUD operations)
- 🔗 Entity mapping system for linking addresses to any entity type
- 🗄️ PostgreSQL optimized with proper indexing
- 🔧 Extensible SQL provider system for custom queries
- 📦 Clean separation of concerns with repository pattern
- 🧪 Comprehensive test suite with integration tests
- 🐳 Docker support for development and deployment
- 📋 Full OpenAPI/Swagger documentation support

## Quick Start

### Installation

```bash
dotnet add package AddressPackage
```

### Basic Setup

```csharp
using AddressPackage.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add Address Package with connection string
builder.Services.AddAddressPackage(connectionString);

var app = builder.Build();
```

### Using the Service

```csharp
[ApiController]
[Route("api/[controller]")]
public class AddressController : ControllerBase
{
    private readonly IAddressService _addressService;

    public AddressController(IAddressService addressService)
    {
        _addressService = addressService;
    }

    [HttpPost]
    public async Task<ActionResult<Address>> CreateAddress(CreateAddressRequest request)
    {
        var address = await _addressService.CreateAddressAsync(request);
        return Created($"/api/address/{address.Id}", address);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Address>> GetAddress(Guid id)
    {
        var address = await _addressService.GetAddressAsync(id);
        return address == null ? NotFound() : Ok(address);
    }
}
```

## Architecture

### Models

- **Address**: Core address entity with street, city, state, postal code, country
- **AddressEntityMapping**: Join table for linking addresses to any entity
- **CreateAddressRequest/UpdateAddressRequest**: DTOs for API operations

### Services

- **IAddressService**: High-level business logic interface
- **IAddressRepository**: Data access interface
- **ISqlProvider**: Extensible SQL query provider

## Database Schema

The package creates two main tables:

```sql
-- Addresses table
CREATE TABLE addresses (
    id UUID PRIMARY KEY,
    street VARCHAR(255) NOT NULL,
    street2 VARCHAR(255),
    city VARCHAR(100) NOT NULL,
    state VARCHAR(100) NOT NULL,
    postal_code VARCHAR(20) NOT NULL,
    country VARCHAR(100) NOT NULL DEFAULT 'USA',
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Entity mappings table
CREATE TABLE address_entity_mappings (
    id UUID PRIMARY KEY,
    address_id UUID NOT NULL REFERENCES addresses(id) ON DELETE CASCADE,
    entity_id UUID NOT NULL,
    entity_type VARCHAR(100) NOT NULL,
    address_type VARCHAR(50),
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    UNIQUE(address_id, entity_id, entity_type)
);
```

## Extensibility

### Custom SQL Provider

You can override default SQL queries by implementing `ISqlProvider`:

```csharp
public class CustomSqlProvider : PostgreSqlProvider
{
    public override string GetCreateAddressQuery()
    {
        return @"
            INSERT INTO addresses (id, street, city, state, postal_code, country, created_at, updated_at)
            VALUES (@Id, @Street, @City, @State, @PostalCode, @Country, @CreatedAt, @UpdatedAt)
            RETURNING *, 'custom' as source;";
    }
}

// Register custom provider
builder.Services.AddAddressPackage<CustomSqlProvider>(connectionString);
```

### Entity Mapping Examples

Link addresses to your entities:

```csharp
// Link address to a customer
await _addressService.AssignAddressToEntityAsync(
    addressId: addressId,
    entityId: customerId, 
    entityType: "Customer",
    addressType: "billing"
);

// Get all addresses for a customer
var customerAddresses = await _addressService.GetAddressesByEntityAsync(
    customerId, 
    "Customer"
);
```

## Development

### Prerequisites

- .NET 9.0 SDK
- Docker and Docker Compose
- Make (optional, for using Makefile commands)

### Setup Development Environment

```bash
# Clone the repository
git clone <repository-url>
cd AddressPackage

# Start PostgreSQL containers
make docker-up
# or
docker-compose up -d postgres postgres_test

# Run tests
make test
# or
dotnet test

# Build package
make build
# or
dotnet build
```

### Available Make Commands

```bash
make help           # Show all available commands
make build          # Build the package
make test           # Run all tests
make test-integration # Run integration tests
make pack           # Create NuGet package
make docker-up      # Start development environment
make docker-down    # Stop development environment
make dev            # Start development with hot reload
make clean          # Clean build artifacts
```

### Running the Sample Consumer API

```bash
# Start the sample API
make dev-build
# or
docker-compose up consumer-api

# The API will be available at http://localhost:5000
# Swagger UI at http://localhost:5000/swagger
```

## Testing

### Unit Tests
```bash
dotnet test --filter Category!=Integration
```

### Integration Tests
Integration tests use Testcontainers to spin up PostgreSQL instances:

```bash
dotnet test --filter Category=Integration
```

### Sample API Endpoints

The sample consumer API provides these endpoints:

- `GET /addresses/{id}` - Get address by ID
- `POST /addresses` - Create new address
- `PUT /addresses/{id}` - Update address
- `DELETE /addresses/{id}` - Delete address
- `GET /entities/{entityId}/addresses` - Get addresses by entity
- `POST /addresses/{addressId}/assign` - Assign address to entity
- `DELETE /addresses/{addressId}/unassign` - Remove address from entity
- `GET /addresses/search` - Search addresses

## Deployment

### Building and Publishing

```bash
# Build and create NuGet package
make pack PACKAGE_VERSION=1.0.0

# Publish to NuGet (requires API key)
make publish PACKAGE_VERSION=1.0.0
```

### Docker Deployment

```bash
# Build consumer API image
make docker-build DOCKER_REGISTRY=your-registry.com

# Push to registry
make docker-push DOCKER_REGISTRY=your-registry.com
```

### Versioning

```bash
make version-patch  # Increment patch version (1.0.0 -> 1.0.1)
make version-minor  # Increment minor version (1.0.0 -> 1.1.0)
make version-major  # Increment major version (1.0.0 -> 2.0.0)
```

## Configuration

### Connection String

```csharp
// appsettings.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=addressdb;Username=user;Password=pass"
  }
}

// Startup configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddAddressPackage(connectionString);
```

### Logging

The package uses `Microsoft.Extensions.Logging` for comprehensive logging:

```csharp
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
    logging.SetMinimumLevel(LogLevel.Information);
});
```

## Best Practices

1. **Entity Types**: Use consistent entity type names across your services
2. **Address Types**: Utilize address types like "billing", "shipping", "primary" for organization
3. **Validation**: The package provides basic validation, but implement additional business rules as needed
4. **Indexing**: The default schema includes indexes for common queries
5. **Connection Management**: The repository properly manages database connections and disposal

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Make your changes
4. Add tests for new functionality
5. Ensure all tests pass (`make test`)
6. Run code formatting (`make lint`)
7. Commit your changes (`git commit -m 'Add amazing feature'`)
8. Push to the branch (`git push origin feature/amazing-feature`)
9. Open a Pull Request

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Support

For issues, questions, or contributions, please open an issue on the GitHub repository.

## Rebuild Solution 
dotnet new sln --name RoBrosAddressesService --force        
dotnet sln add **/*.csproj