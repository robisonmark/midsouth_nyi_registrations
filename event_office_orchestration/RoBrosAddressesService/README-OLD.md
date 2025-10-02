# RoBros Address Service
## Using RoBros Address Service
### If publishing to NuGet
```bash
dotnet add package YourCompany.AddressService
```

### If using local package
```bash
dotnet add reference ../path/to/YourCompany.AddressService/src/YourCompany.AddressService/YourCompany.AddressService.csproj
```

### **Standard Usage (Address Service Controls Everything):**

**OrderService.Api/Program.cs:**
```csharp
using YourCompany.AddressService.Extensions;
using Microsoft.Data.SqlConnection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Standard usage - AddressService controls schema completely
builder.Services.AddAddressService(
    serviceProvider => new SqlConnection(builder.Configuration.GetConnectionString("SharedDatabase")),
    options =>
    {
        options.AddressTableName = "Order_Addresses";
        options.JoinTableName = "Order_EntityAddresses";
        // AddressService will create and manage the schema
    });

var app = builder.Build();
```

**OrderService.Api/Controllers/OrdersController.cs:**
```csharp
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IAddressService _addressService;
    
    public OrdersController(IAddressService addressService)
    {
        _addressService = addressService;
    }
    
    [HttpPost("{orderId}/shipping-address")]
    public async Task<IActionResult> SetShippingAddress(int orderId, CreateAddressRequest request)
    {
        // AddressService handles everything - just returns the ID
        var addressId = await _addressService.CreateAddressAsync(
            request, 
            new EntityInfo 
            { 
                EntityType = "orders",
                EntityId = orderId,
                RelationshipType = "shipping"
            });
            
        return Ok(new { AddressId = addressId }); // Only returns the ID!
    }
}
```

### **Advanced Usage (Service Overrides Schema):**

**CustomerService.Api/Program.cs:**
```csharp
using YourCompany.AddressService.Extensions;
using Microsoft.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Advanced usage - Override schema for special requirements
builder.Services.AddAddressService(
    serviceProvider => new SqlConnection(builder.Configuration.GetConnectionString("SharedDatabase")),
    options =>
    {
        options.AddressTableName = "Customer_Addresses";
        options.JoinTableName = "Customer_EntityAddresses";
        options.AllowSchemaOverride = true;  // Allow overriding existing schema
        
        // Add custom columns specific to customer domain
        options.AdditionalColumns.Add("CustomerTier", "NVARCHAR(50) NULL");
        options.AdditionalColumns.Add("IsCommercial", "BIT NOT NULL DEFAULT 0");
        options.AdditionalColumns.Add("TaxExemptNumber", "NVARCHAR(50) NULL");
        
        // Add custom indexes for customer-specific queries
        options.CustomIndexes.Add(
            $"CREATE INDEX IX_Customer_Addresses_Tier ON Customer_Addresses(CustomerTier, IsActive)"
        );
    });

var app = builder.Build();
```

**CustomerService.Api/Controllers/CustomersController.cs:**
```csharp
[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly IAddressService _addressService;
    
    public CustomersController(IAddressService addressService)
    {
        _addressService = addressService;
    }
    
    [HttpPost("{customerId}/addresses")]
    public async Task<IActionResult> CreateCustomerAddress(int customerId, CreateCustomerAddressRequest request)
    {
        // Even with custom schema, still just returns the ID
        var addressId = await _addressService.CreateAddressAsync(
            new CreateAddressRequest
            {
                StreetAddress = request.StreetAddress,
                City = request.City,
                State = request.State,
                PostalCode = request.PostalCode,
                Country = request.Country
            }, 
            new EntityInfo 
            { 
                EntityType = "customers",
                EntityId = customerId,
                RelationshipType = request.IsCommercial ? "commercial" : "residential"
            });

        // Handle custom fields separately if needed
        if (request.CustomerTier != null || request.IsCommercial || request.TaxExemptNumber != null)
        {
            await UpdateCustomAddressFields(addressId, request.CustomerTier, request.IsCommercial, request.TaxExemptNumber);
        }
            
        return Ok(new { AddressId = addressId });
    }
    
    private async Task UpdateCustomAddressFields(int addressId, string? customerTier, bool isCommercial, string? taxExemptNumber)
    {
        // Custom update logic for the additional columns
        // This is domain-specific business logic that the AddressService doesn't need to know about
    }
}

public class CreateCustomerAddressRequest : CreateAddressRequest
{
    public string? CustomerTier { get; set; }
    public bool IsCommercial { get; set; }
    public string? TaxExemptNumber { get; set; }
}
```

### **Migration-Safe Usage (Validate Before Override):**

**UserService.Api/Program.cs:**
```csharp
using YourCompany.AddressService.Extensions;
using Microsoft.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Migration-safe usage - Check existing schema first
builder.Services.AddAddressService(
    serviceProvider => new SqlConnection(builder.Configuration.GetConnectionString("SharedDatabase")),
    options =>
    {
        options.AddressTableName = "User_Addresses";
        options.JoinTableName = "User_EntityAddresses";
        options.AllowSchemaOverride = false;  // Protect existing schema
        // If schema exists and doesn't match, will throw exception
        // If schema doesn't exist, will create it
    });

// Add initialization check at startup
builder.Services.AddHostedService<AddressServiceInitializationService>();

var app = builder.Build();
```

**AddressServiceInitializationService.cs:**
```csharp
public class AddressServiceInitializationService : IHostedService
{
    private readonly IAddressService _addressService;
    private readonly ILogger<AddressServiceInitializationService> _logger;

    public AddressServiceInitializationService(
        IAddressService addressService, 
        ILogger<AddressServiceInitializationService> logger)
    {
        _addressService = addressService;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Initializing Address Service schema...");
            
            var schemaExists = await _addressService.SchemaExistsAsync();
            if (schemaExists)
            {
                var isValid = await _addressService.ValidateSchemaAsync();
                if (isValid)
                {
                    _logger.LogInformation("Address Service schema validated successfully");
                }
                else
                {
                    _logger.LogWarning("Address Service schema validation failed - may need migration");
                }
            }
            else
            {
                _logger.LogInformation("Creating Address Service schema...");
                await _addressService.CreateSchemaAsync();
                _logger.LogInformation("Address Service schema created successfully");
            }
            
            await _addressService.InitializeAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize Address Service");
            throw; // Fail startup if address service can't initialize
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
```

### **Key Benefits of This Approach:**

1. **🎯 Consistent Core**: AddressService owns the essential schema and business logic
2. **🔧 Flexible Overrides**: Domains can add custom columns/indexes when needed
3. **📦 Simple Integration**: Most services just get ID back - no complex models to manage
4. **🛡️ Schema Protection**: Can prevent accidental schema changes in production
5. **⚡ Performance**: Custom indexes per domain for optimized queries
6. **🔄 Migration Safe**: Validates existing schema before making changes

### **What Each Service Gets:**

- **Standard Services**: Just call `CreateAddressAsync()` → get back Address ID
- **Advanced Services**: Custom schema + same simple API → still get back Address ID  
- **All Services**: Consistent address validation, join table management, and data integrity
- **Database**: Single shared database with domain-specific tables and optimizations

The AddressService **owns** the address creation process and **guarantees** you get a valid Address ID back, but each domain can still customize the schema for their specific needs!# Address Service Package Structure with Docker Development

## Complete Directory Layout

```
YourCompany.AddressService/
├── src/
│   └── YourCompany.AddressService/
│       ├── Models/
│       │   ├── Address.cs
│       │   ├── EntityAddress.cs
│       │   ├── CreateAddressRequest.cs
│       │   ├── UpdateAddressRequest.cs
│       │   ├── EntityInfo.cs
│       │   ├── SearchAddressCriteria.cs
│       │   ├── SearchOptions.cs
│       │   ├── GetAddressesOptions.cs
│       │   ├── LinkResult.cs
│       │   └── AddressStats.cs
│       ├── Services/
│       │   ├── IAddressService.cs
│       │   └── AddressService.cs              # 🎯 ALL YOUR LOGIC GOES HERE
│       ├── Configuration/
│       │   └── AddressServiceOptions.cs
│       ├── Extensions/
│       │   └── ServiceCollectionExtensions.cs
│       ├── Validation/
│       │   ├── AddressValidator.cs             # Business rule validation
│       │   └── ValidationExtensions.cs
│       ├── Exceptions/
│       │   ├── AddressServiceException.cs
│       │   └── SchemaValidationException.cs
│       └── YourCompany.AddressService.csproj
├── tests/
│   └── YourCompany.AddressService.Tests/
│       ├── Unit/
│       │   ├── AddressServiceTests.cs
│       │   ├── AddressValidatorTests.cs
│       │   └── SchemaManagementTests.cs
│       ├── Integration/
│       │   ├── DatabaseIntegrationTests.cs
│       │   └── SchemaCreationTests.cs
│       ├── TestFixtures/
│       │   └── DatabaseFixture.cs
│       └── YourCompany.AddressService.Tests.csproj
├── samples/
│   └── AddressService.Sample/
│       ├── Program.cs                          # 🧪 ISOLATED TESTING APP
│       ├── Controllers/
│       │   └── AddressController.cs
│       ├── appsettings.json
│       ├── appsettings.Development.json
│       └── AddressService.Sample.csproj
├── docker/
│   ├── docker-compose.yml                      # 🐳 ISOLATED DEVELOPMENT ENVIRONMENT
│   ├── docker-compose.override.yml
│   ├── sqlserver/
│   │   └── init.sql
│   └── Dockerfile.sample
├── scripts/
│   ├── build.sh
│   ├── test.sh
│   ├── start-dev.sh
│   └── reset-db.sh
├── docs/
│   ├── README.md
│   ├── API.md
│   └── DEVELOPMENT.md
├── .env
├── .gitignore
├── Directory.Build.props
├── YourCompany.AddressService.sln
└── README.md
```

## 🎯 Where All Your Logic Lives

### **YourCompany.AddressService/Services/AddressService.cs**
This is where ALL your address business logic goes:

```csharp
public class AddressService : IAddressService
{
    // 🏗️ Schema Management Logic
    private async Task CreateSchemaAsync() { /* Schema creation logic */ }
    private async Task<bool> ValidateSchemaAsync() { /* Schema validation logic */ }
    
    // 📝 Address Business Logic
    public async Task<int> CreateAddressAsync(CreateAddressRequest request, EntityInfo? entityInfo = null)
    {
        // Validation logic
        await ValidateAddressRequest(request);
        
        // Normalization logic  
        var normalizedAddress = await NormalizeAddress(request);
        
        // Duplicate detection logic
        var existingId = await CheckForDuplicate(normalizedAddress);
        if (existingId.HasValue) return existingId.Value;
        
        // Creation logic
        return await CreateNewAddress(normalizedAddress, entityInfo);
    }
    
    // 🔍 Address Validation Logic
    private async Task ValidateAddressRequest(CreateAddressRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.PostalCode))
            throw new AddressServiceException("Postal code is required");
            
        if (!await IsValidPostalCodeFormat(request.PostalCode, request.Country))
            throw new AddressServiceException("Invalid postal code format");
            
        // Add more validation rules
    }
    
    // 🧹 Address Normalization Logic
    private async Task<CreateAddressRequest> NormalizeAddress(CreateAddressRequest request)
    {
        return new CreateAddressRequest
        {
            StreetAddress = NormalizeStreetAddress(request.StreetAddress),
            City = NormalizeCity(request.City),
            State = NormalizeState(request.State, request.Country),
            PostalCode = NormalizePostalCode(request.PostalCode, request.Country),
            Country = request.Country?.ToUpper() ?? "US"
        };
    }
    
    // 🔄 Duplicate Detection Logic
    private async Task<int?> CheckForDuplicate(CreateAddressRequest normalized)
    {
        // Your duplicate detection algorithm
        var sql = $@"
            SELECT TOP 1 Id FROM [{_options.AddressTableName}]
            WHERE StreetAddress = @StreetAddress 
              AND City = @City 
              AND State = @State 
              AND PostalCode = @PostalCode
              AND Country = @Country
              AND IsActive = 1";
              
        var results = await QueryAsync<int?>(sql, normalized);
        return results.FirstOrDefault();
    }
    
    // ... all other address logic methods
}
```

## 🐳 Docker Development Environment

### **docker/docker-compose.yml**
```yaml
version: '3.8'

services:
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    container_name: addressservice-sqlserver
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=YourStrong@Password123
      - MSSQL_PID=Developer
    ports:
      - "1434:1433"
    volumes:
      - sqlserver_data:/var/opt/mssql
      - ./sqlserver/init.sql:/docker-entrypoint-initdb.d/init.sql:ro
    networks:
      - addressservice-network

  addressservice-sample:
    build:
      context: ..
      dockerfile: docker/Dockerfile.sample
    container_name: addressservice-sample
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__SharedDatabase=Server=sqlserver,1433;Database=AddressServiceTest;User Id=sa;Password=YourStrong@Password123;TrustServerCertificate=true;
    ports:
      - "5000:8080"
    depends_on:
      - sqlserver
    volumes:
      - ../samples/AddressService.Sample:/app
      - ../src:/src
    networks:
      - addressservice-network

volumes:
  sqlserver_data:

networks:
  addressservice-network:
    driver: bridge
```

### **docker/Dockerfile.sample**
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files
COPY ["samples/AddressService.Sample/AddressService.Sample.csproj", "samples/AddressService.Sample/"]
COPY ["src/YourCompany.AddressService/YourCompany.AddressService.csproj", "src/YourCompany.AddressService/"]

# Restore dependencies
RUN dotnet restore "samples/AddressService.Sample/AddressService.Sample.csproj"

# Copy everything else
COPY . .

# Build the sample app
WORKDIR "/src/samples/AddressService.Sample"
RUN dotnet build "AddressService.Sample.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AddressService.Sample.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AddressService.Sample.dll"]
```

### **samples/AddressService.Sample/Program.cs** 
```csharp
using YourCompany.AddressService.Extensions;
using Microsoft.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add the address service for isolated testing
builder.Services.AddAddressService(
    serviceProvider => new SqlConnection(builder.Configuration.GetConnectionString("SharedDatabase")),
    options =>
    {
        options.AddressTableName = "Test_Addresses";
        options.JoinTableName = "Test_EntityAddresses";
        options.AllowSchemaOverride = true;
    });

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

// Initialize the schema on startup
using (var scope = app.Services.CreateScope())
{
    var addressService = scope.ServiceProvider.GetRequiredService<IAddressService>();
    await addressService.InitializeAsync();
}

app.Run();
```

### **samples/AddressService.Sample/Controllers/AddressController.cs**
```csharp
using Microsoft.AspNetCore.Mvc;
using YourCompany.AddressService.Services;
using YourCompany.AddressService.Models;

namespace AddressService.Sample.Controllers;

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
    public async Task<IActionResult> CreateAddress(CreateAddressRequest request)
    {
        var addressId = await _addressService.CreateAddressAsync(request);
        return Ok(new { AddressId = addressId });
    }
    
    [HttpPost("{addressId}/link")]
    public async Task<IActionResult> LinkToEntity(int addressId, EntityInfo entityInfo)
    {
        var linkedAddressId = await _addressService.LinkAddressToEntityAsync(addressId, entityInfo);
        return Ok(new { AddressId = linkedAddressId });
    }
    
    [HttpGet("{addressId}")]
    public async Task<IActionResult> GetAddress(int addressId)
    {
        var address = await _addressService.GetAddressByIdAsync(addressId);
        if (address == null) return NotFound();
        return Ok(address);
    }
    
    [HttpGet("entity/{entityType}/{entityId}")]
    public async Task<IActionResult> GetEntityAddresses(string entityType, int entityId)
    {
        var addresses = await _addressService.GetEntityAddressesAsync(entityType, entityId);
        return Ok(addresses);
    }
    
    [HttpPost("search")]
    public async Task<IActionResult> SearchAddresses(SearchAddressCriteria criteria, [FromQuery] SearchOptions? options = null)
    {
        var addresses = await _addressService.SearchAddressesAsync(criteria, options);
        return Ok(addresses);
    }
}
```

## 🚀 Development Workflow

### **Start Isolated Development Environment:**
```bash
# Clone your address service repo
git clone https://your-repo/YourCompany.AddressService.git
cd YourCompany.AddressService

# Start the development environment
chmod +x scripts/start-dev.sh
./scripts/start-dev.sh
```

### **scripts/start-dev.sh**
```bash
#!/bin/bash

echo "🚀 Starting Address Service Development Environment..."

# Build and start containers
docker-compose -f docker/docker-compose.yml up -d

# Wait for SQL Server to be ready
echo "⏳ Waiting for SQL Server to be ready..."
sleep 30

# Run database initialization if needed
echo "🗄️ Initializing database..."
docker-compose -f docker/docker-compose.yml exec sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P YourStrong@Password123 -Q "CREATE DATABASE AddressServiceTest"

echo "✅ Development environment ready!"
echo "📊 Swagger UI: http://localhost:5000/swagger"
echo "🗄️ SQL Server: localhost:1434 (sa/YourStrong@Password123)"
```

### **Test Your Logic Locally:**
```bash
# Run unit tests
./scripts/test.sh

# Test specific functionality
curl -X POST http://localhost:5000/api/address \
  -H "Content-Type: application/json" \
  -d '{
    "streetAddress": "123 Main St",
    "city": "New York", 
    "state": "NY",
    "postalCode": "10001"
  }'
```

## 🧪 Benefits of This Structure

1. **🎯 Logic Centralization**: All address logic in one place - `AddressService.cs`
2. **🐳 Isolated Development**: Docker environment independent of other services  
3. **🧪 Easy Testing**: Sample app for manual testing, unit tests for automated testing
4. **📦 Clean Packaging**: Ready to publish as NuGet package
5. **🔄 Fast Iteration**: Make changes, rebuild, test immediately
6. **📊 Full Observability**: Swagger UI, logs, database access for debugging

You develop the AddressService in complete isolation, test all your logic thoroughly, then publish it as a package that other services can consume!

## Key Files

### 1. **YourCompany.AddressService.csproj**
```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <LangVersion>latest</LangVersion>
    <Nullable>enable</Nullable>
    <GeneratePackageOnBuild>true</GeneratePackageOnBuild>
    <PackageId>YourCompany.AddressService</PackageId>
    <PackageVersion>1.0.0</PackageVersion>
    <Authors>Your Company</Authors>
    <Description>Self-contained address service library for .NET applications</Description>
    <PackageTags>address;service;library;database</PackageTags>
    <RepositoryUrl>https://github.com/yourcompany/address-service</RepositoryUrl>
    <PackageLicenseExpression>MIT</PackageLicenseExpression>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.Extensions.DependencyInjection.Abstractions" Version="9.0.0" />
    <PackageReference Include="Microsoft.Extensions.Logging.Abstractions" Version="9.0.0" />
    <PackageReference Include="System.Data.Common" Version="4.3.0" />
  </ItemGroup>

</Project>
```

### 2. **ServiceCollectionExtensions.cs**
```csharp
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
```

### 3. **IAddressService.cs** (Interface)
```csharp
using YourCompany.AddressService.Models;

namespace YourCompany.AddressService.Services;

public interface IAddressService
{
    Task InitializeAsync();
    Task<Address> CreateAddressAsync(CreateAddressRequest addressData, EntityInfo? entityInfo = null);
    Task<LinkResult> LinkAddressToEntityAsync(int addressId, EntityInfo entityInfo);
    Task<List<EntityAddress>> GetEntityAddressesAsync(string entityType, int entityId, GetAddressesOptions? options = null);
    Task<Address?> GetAddressByIdAsync(int addressId);
    Task<Address?> UpdateAddressAsync(int addressId, UpdateAddressRequest updateData);
    Task<bool> DeactivateAddressAsync(int addressId);
    Task<bool> UnlinkAddressFromEntityAsync(int addressId, string entityType, int entityId, string? relationshipType = null);
    Task<List<Address>> SearchAddressesAsync(SearchAddressCriteria searchCriteria, SearchOptions? options = null);
    Task<AddressStats> GetStatsAsync();
}
```

### 4. **Directory.Build.props**
```xml
<Project>
  <PropertyGroup>
    <LangVersion>latest</LangVersion>
    <Nullable>enable</Nullable>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <WarningsAsErrors />
    <WarningsNotAsErrors>NU1701</WarningsNotAsErrors>
  </PropertyGroup>
</Project>
```

## Usage in Other Services

### 1. **Install the Package**
```bash
# If publishing to NuGet
dotnet add package YourCompany.AddressService

# If using local package
dotnet add reference ../path/to/YourCompany.AddressService/src/YourCompany.AddressService/YourCompany.AddressService.csproj
```

### 2. **Register in DI Container (Program.cs)**
```csharp
using YourCompany.AddressService.Extensions;
using Microsoft.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);

// Register with connection factory
builder.Services.AddAddressService(
    serviceProvider => new SqlConnection(builder.Configuration.GetConnectionString("DefaultConnection")),
    options =>
    {
        options.AddressTableName = "CustomAddresses";
        options.JoinTableName = "CustomEntityAddresses";
    });

var app = builder.Build();
```

### 3. **Use in Your Controllers/Services**
```csharp
using YourCompany.AddressService.Services;
using YourCompany.AddressService.Models;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IAddressService _addressService;
    
    public UsersController(IAddressService addressService)
    {
        _addressService = addressService;
    }
    
    [HttpPost("{userId}/addresses")]
    public async Task<IActionResult> CreateUserAddress(int userId, CreateAddressRequest request)
    {
        var address = await _addressService.CreateAddressAsync(
            request, 
            new EntityInfo 
            { 
                EntityType = "users", 
                EntityId = userId 
            });
            
        return Ok(new { AddressId = address.Id });
    }
}
```

## Cost-Effective Architecture Benefits

### **Single Database Advantages:**
1. **💰 Much Lower Costs**: One database instance vs multiple
2. **📈 Easy Scaling**: Scale up one database during peak season
3. **🔧 Simplified Management**: One connection pool, one backup strategy
4. **⚡ Better Performance**: Cross-domain queries possible if needed
5. **🛡️ Data Isolation**: Still logically separated by table prefixes

### **Seasonal Scaling Strategy:**
```csharp
// appsettings.Production.json for peak season
{
  "ConnectionStrings": {
    "SharedDatabase": "Server=high-performance-server;Database=MyAppDatabase;Connection Timeout=30;Max Pool Size=200;"
  }
}

// appsettings.json for off-season  
{
  "ConnectionStrings": {
    "SharedDatabase": "Server=basic-server;Database=MyAppDatabase;Connection Timeout=30;Max Pool Size=50;"
  }
}
```

### **Example Domain Structure (Cost-Optimized):**
```
YourCompany.Services/
├── UserService.Api/              # dotnet new webapi
├── OrderService.Api/             # dotnet new webapi  
├── CustomerService.Api/          # dotnet new webapi
├── ProductService.Api/           # dotnet new webapi
└── All connect to same database with different table prefixes
```

### **Database Growth Pattern:**
- **Off-Season**: Small database, basic hosting
- **Peak Season**: Scale up to premium database tier
- **Consistent**: All services still work identically
- **Future**: Easy to split databases later if needed

## Alternative Distribution Methods

### 1. **NuGet Package** (Recommended)
- Build and publish to your private NuGet feed
- Easy versioning and updates
- Dependency management

### 2. **Git Submodule**
- Include as a submodule in consuming projects
- Direct source access
- Harder to version

### 3. **Local Package Reference**
- Reference the `.csproj` directly
- Good for development and testing
- Easy to modify and debug


---
using Microsoft.Data.SqlClient;
using System.Data;
using System.Data.Common;
using System.Text;

namespace AddressService
{
    public class AddressServiceOptions
    {
        public string AddressTableName { get; set; } = "Addresses";
        public string JoinTableName { get; set; } = "EntityAddresses";
    }

    public class Address
    {
        public int Id { get; set; }
        public string StreetAddress { get; set; } = string.Empty;
        public string? Apartment { get; set; }
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string Country { get; set; } = "US";
        public string AddressType { get; set; } = "billing";
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class EntityAddress : Address
    {
        public string RelationshipType { get; set; } = string.Empty;
        public bool RelationshipActive { get; set; }
    }

    public class CreateAddressRequest
    {
        public string StreetAddress { get; set; } = string.Empty;
        public string? Apartment { get; set; }
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string Country { get; set; } = "US";
        public string AddressType { get; set; } = "billing";
    }

    public class EntityInfo
    {
        public string EntityType { get; set; } = string.Empty;
        public int EntityId { get; set; }
        public string RelationshipType { get; set; } = "primary";
    }

    public class UpdateAddressRequest
    {
        public string? StreetAddress { get; set; }
        public string? Apartment { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }
        public string? Country { get; set; }
        public string? AddressType { get; set; }
    }

    public class SearchAddressCriteria
    {
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }
        public string? Country { get; set; }
        public string? AddressType { get; set; }
    }

    public class SearchOptions
    {
        public int Limit { get; set; } = 50;
        public int Offset { get; set; } = 0;
        public bool IncludeInactive { get; set; } = false;
    }

    public class GetAddressesOptions
    {
        public string? RelationshipType { get; set; }
        public bool IncludeInactive { get; set; } = false;
    }

    public class LinkResult
    {
        public bool Success { get; set; }
        public int AddressId { get; set; }
        public string EntityType { get; set; } = string.Empty;
        public int EntityId { get; set; }
        public string RelationshipType { get; set; } = string.Empty;
    }

    public class AddressStats
    {
        public int TotalAddresses { get; set; }
        public int TotalRelationships { get; set; }
    }

    public class AddressService
    {
        private readonly IDbConnection _connection;
        private readonly AddressServiceOptions _options;
        private bool _initialized = false;

        public AddressService(IDbConnection connection, AddressServiceOptions? options = null)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _options = options ?? new AddressServiceOptions();
        }

        /// <summary>
        /// Initialize the address service - creates necessary tables if they don't exist
        /// </summary>
        public async Task InitializeAsync()
        {
            if (_initialized) return;

            try
            {
                await EnsureConnectionOpenAsync();

                // Create addresses table
                var createAddressesTable = $@"
                    IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='{_options.AddressTableName}' AND xtype='U')
                    CREATE TABLE [{_options.AddressTableName}] (
                        Id INT IDENTITY(1,1) PRIMARY KEY,
                        StreetAddress NVARCHAR(255) NOT NULL,
                        Apartment NVARCHAR(100) NULL,
                        City NVARCHAR(100) NOT NULL,
                        State NVARCHAR(100) NOT NULL,
                        PostalCode NVARCHAR(20) NOT NULL,
                        Country NVARCHAR(100) NOT NULL DEFAULT 'US',
                        AddressType NVARCHAR(50) NOT NULL DEFAULT 'billing',
                        IsActive BIT NOT NULL DEFAULT 1,
                        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                        UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
                    )";

                await ExecuteNonQueryAsync(createAddressesTable);

                // Create join table for entity-address relationships
                var createJoinTable = $@"
                    IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='{_options.JoinTableName}' AND xtype='U')
                    CREATE TABLE [{_options.JoinTableName}] (
                        Id INT IDENTITY(1,1) PRIMARY KEY,
                        EntityType NVARCHAR(100) NOT NULL,
                        EntityId INT NOT NULL,
                        AddressId INT NOT NULL,
                        RelationshipType NVARCHAR(50) NOT NULL DEFAULT 'primary',
                        IsActive BIT NOT NULL DEFAULT 1,
                        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                        FOREIGN KEY (AddressId) REFERENCES [{_options.AddressTableName}](Id) ON DELETE CASCADE,
                        UNIQUE (EntityType, EntityId, AddressId, RelationshipType)
                    )";

                await ExecuteNonQueryAsync(createJoinTable);

                // Create indexes for better performance
                var createIndexes = $@"
                    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_{_options.JoinTableName}_Entity')
                    CREATE INDEX IX_{_options.JoinTableName}_Entity ON [{_options.JoinTableName}](EntityType, EntityId);

                    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_{_options.JoinTableName}_Address')
                    CREATE INDEX IX_{_options.JoinTableName}_Address ON [{_options.JoinTableName}](AddressId);";

                await ExecuteNonQueryAsync(createIndexes);

                _initialized = true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to initialize address service: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Create a new address and optionally link it to an entity
        /// </summary>
        public async Task<Address> CreateAddressAsync(CreateAddressRequest addressData, EntityInfo? entityInfo = null)
        {
            await InitializeAsync();

            // Validate required fields
            if (string.IsNullOrWhiteSpace(addressData.StreetAddress) ||
                string.IsNullOrWhiteSpace(addressData.City) ||
                string.IsNullOrWhiteSpace(addressData.State) ||
                string.IsNullOrWhiteSpace(addressData.PostalCode))
            {
                throw new ArgumentException("Missing required address fields: StreetAddress, City, State, PostalCode");
            }

            await EnsureConnectionOpenAsync();

            using var transaction = _connection.BeginTransaction();
            try
            {
                // Insert address
                var insertAddressSql = $@"
                    INSERT INTO [{_options.AddressTableName}] 
                    (StreetAddress, Apartment, City, State, PostalCode, Country, AddressType)
                    VALUES (@StreetAddress, @Apartment, @City, @State, @PostalCode, @Country, @AddressType);
                    SELECT CAST(SCOPE_IDENTITY() as int);";

                var addressId = await ExecuteScalarAsync<int>(insertAddressSql, new
                {
                    StreetAddress = addressData.StreetAddress,
                    Apartment = addressData.Apartment,
                    City = addressData.City,
                    State = addressData.State,
                    PostalCode = addressData.PostalCode,
                    Country = addressData.Country,
                    AddressType = addressData.AddressType
                }, transaction);

                // If entity info provided, create the relationship
                if (entityInfo != null)
                {
                    if (string.IsNullOrWhiteSpace(entityInfo.EntityType) || entityInfo.EntityId <= 0)
                    {
                        throw new ArgumentException("Entity type and ID are required when linking address");
                    }

                    var linkSql = $@"
                        INSERT INTO [{_options.JoinTableName}] 
                        (EntityType, EntityId, AddressId, RelationshipType)
                        VALUES (@EntityType, @EntityId, @AddressId, @RelationshipType)";

                    await ExecuteNonQueryAsync(linkSql, new
                    {
                        EntityType = entityInfo.EntityType,
                        EntityId = entityInfo.EntityId,
                        AddressId = addressId,
                        RelationshipType = entityInfo.RelationshipType
                    }, transaction);
                }

                transaction.Commit();

                // Return the created address
                return await GetAddressByIdAsync(addressId) 
                    ?? throw new InvalidOperationException("Failed to retrieve created address");
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        /// <summary>
        /// Link an existing address to an entity
        /// </summary>
        public async Task<LinkResult> LinkAddressToEntityAsync(int addressId, EntityInfo entityInfo)
        {
            await InitializeAsync();

            if (string.IsNullOrWhiteSpace(entityInfo.EntityType) || entityInfo.EntityId <= 0 || addressId <= 0)
            {
                throw new ArgumentException("Entity type, entity ID, and address ID are required");
            }

            try
            {
                var sql = $@"
                    MERGE [{_options.JoinTableName}] AS target
                    USING (SELECT @EntityType AS EntityType, @EntityId AS EntityId, @AddressId AS AddressId, @RelationshipType AS RelationshipType) AS source
                    ON target.EntityType = source.EntityType 
                       AND target.EntityId = source.EntityId 
                       AND target.AddressId = source.AddressId 
                       AND target.RelationshipType = source.RelationshipType
                    WHEN MATCHED THEN
                        UPDATE SET IsActive = 1, CreatedAt = GETUTCDATE()
                    WHEN NOT MATCHED THEN
                        INSERT (EntityType, EntityId, AddressId, RelationshipType)
                        VALUES (source.EntityType, source.EntityId, source.AddressId, source.RelationshipType);";

                await ExecuteNonQueryAsync(sql, new
                {
                    EntityType = entityInfo.EntityType,
                    EntityId = entityInfo.EntityId,
                    AddressId = addressId,
                    RelationshipType = entityInfo.RelationshipType
                });

                return new LinkResult
                {
                    Success = true,
                    AddressId = addressId,
                    EntityType = entityInfo.EntityType,
                    EntityId = entityInfo.EntityId,
                    RelationshipType = entityInfo.RelationshipType
                };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to link address to entity: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Get addresses for a specific entity
        /// </summary>
        public async Task<List<EntityAddress>> GetEntityAddressesAsync(string entityType, int entityId, GetAddressesOptions? options = null)
        {
            await InitializeAsync();
            options ??= new GetAddressesOptions();

            var sql = new StringBuilder($@"
                SELECT 
                    a.Id, a.StreetAddress, a.Apartment, a.City, a.State, a.PostalCode, 
                    a.Country, a.AddressType, a.CreatedAt, a.UpdatedAt, a.IsActive,
                    ea.RelationshipType, ea.IsActive as RelationshipActive
                FROM [{_options.AddressTableName}] a
                JOIN [{_options.JoinTableName}] ea ON a.Id = ea.AddressId
                WHERE ea.EntityType = @EntityType AND ea.EntityId = @EntityId");

            var parameters = new Dictionary<string, object>
            {
                ["EntityType"] = entityType,
                ["EntityId"] = entityId
            };

            if (!options.IncludeInactive)
            {
                sql.Append(" AND ea.IsActive = 1 AND a.IsActive = 1");
            }

            if (!string.IsNullOrWhiteSpace(options.RelationshipType))
            {
                sql.Append(" AND ea.RelationshipType = @RelationshipType");
                parameters["RelationshipType"] = options.RelationshipType;
            }

            sql.Append(" ORDER BY ea.CreatedAt DESC");

            try
            {
                return await QueryAsync<EntityAddress>(sql.ToString(), parameters);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to get entity addresses: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Get a specific address by ID
        /// </summary>
        public async Task<Address?> GetAddressByIdAsync(int addressId)
        {
            await InitializeAsync();

            try
            {
                var sql = $@"
                    SELECT Id, StreetAddress, Apartment, City, State, PostalCode, 
                           Country, AddressType, IsActive, CreatedAt, UpdatedAt
                    FROM [{_options.AddressTableName}]
                    WHERE Id = @Id AND IsActive = 1";

                var results = await QueryAsync<Address>(sql, new { Id = addressId });
                return results.FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to get address: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Update an existing address
        /// </summary>
        public async Task<Address?> UpdateAddressAsync(int addressId, UpdateAddressRequest updateData)
        {
            await InitializeAsync();

            var updates = new List<string>();
            var parameters = new Dictionary<string, object> { ["Id"] = addressId };

            if (!string.IsNullOrWhiteSpace(updateData.StreetAddress))
            {
                updates.Add("StreetAddress = @StreetAddress");
                parameters["StreetAddress"] = updateData.StreetAddress;
            }

            if (updateData.Apartment != null)
            {
                updates.Add("Apartment = @Apartment");
                parameters["Apartment"] = updateData.Apartment;
            }

            if (!string.IsNullOrWhiteSpace(updateData.City))
            {
                updates.Add("City = @City");
                parameters["City"] = updateData.City;
            }

            if (!string.IsNullOrWhiteSpace(updateData.State))
            {
                updates.Add("State = @State");
                parameters["State"] = updateData.State;
            }

            if (!string.IsNullOrWhiteSpace(updateData.PostalCode))
            {
                updates.Add("PostalCode = @PostalCode");
                parameters["PostalCode"] = updateData.PostalCode;
            }

            if (!string.IsNullOrWhiteSpace(updateData.Country))
            {
                updates.Add("Country = @Country");
                parameters["Country"] = updateData.Country;
            }

            if (!string.IsNullOrWhiteSpace(updateData.AddressType))
            {
                updates.Add("AddressType = @AddressType");
                parameters["AddressType"] = updateData.AddressType;
            }

            if (updates.Count == 0)
            {
                throw new ArgumentException("No valid fields to update");
            }

            updates.Add("UpdatedAt = GETUTCDATE()");

            try
            {
                var sql = $@"
                    UPDATE [{_options.AddressTableName}]
                    SET {string.Join(", ", updates)}
                    WHERE Id = @Id AND IsActive = 1";

                await ExecuteNonQueryAsync(sql, parameters);
                return await GetAddressByIdAsync(addressId);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to update address: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Soft delete an address (marks as inactive)
        /// </summary>
        public async Task<bool> DeactivateAddressAsync(int addressId)
        {
            await InitializeAsync();

            try
            {
                var sql = $@"
                    UPDATE [{_options.AddressTableName}]
                    SET IsActive = 0, UpdatedAt = GETUTCDATE()
                    WHERE Id = @Id";

                var rowsAffected = await ExecuteNonQueryAsync(sql, new { Id = addressId });
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to deactivate address: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Remove the link between an address and entity
        /// </summary>
        public async Task<bool> UnlinkAddressFromEntityAsync(int addressId, string entityType, int entityId, string? relationshipType = null)
        {
            await InitializeAsync();

            var sql = new StringBuilder($@"
                UPDATE [{_options.JoinTableName}]
                SET IsActive = 0
                WHERE AddressId = @AddressId AND EntityType = @EntityType AND EntityId = @EntityId");

            var parameters = new Dictionary<string, object>
            {
                ["AddressId"] = addressId,
                ["EntityType"] = entityType,
                ["EntityId"] = entityId
            };

            if (!string.IsNullOrWhiteSpace(relationshipType))
            {
                sql.Append(" AND RelationshipType = @RelationshipType");
                parameters["RelationshipType"] = relationshipType;
            }

            try
            {
                var rowsAffected = await ExecuteNonQueryAsync(sql.ToString(), parameters);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to unlink address from entity: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Search addresses by various criteria
        /// </summary>
        public async Task<List<Address>> SearchAddressesAsync(SearchAddressCriteria searchCriteria, SearchOptions? options = null)
        {
            await InitializeAsync();
            options ??= new SearchOptions();

            var sql = new StringBuilder($"SELECT * FROM [{_options.AddressTableName}] WHERE 1=1");
            var parameters = new Dictionary<string, object>();

            if (!options.IncludeInactive)
            {
                sql.Append(" AND IsActive = 1");
            }

            if (!string.IsNullOrWhiteSpace(searchCriteria.City))
            {
                sql.Append(" AND City LIKE @City");
                parameters["City"] = $"%{searchCriteria.City}%";
            }

            if (!string.IsNullOrWhiteSpace(searchCriteria.State))
            {
                sql.Append(" AND State = @State");
                parameters["State"] = searchCriteria.State;
            }

            if (!string.IsNullOrWhiteSpace(searchCriteria.PostalCode))
            {
                sql.Append(" AND PostalCode = @PostalCode");
                parameters["PostalCode"] = searchCriteria.PostalCode;
            }

            if (!string.IsNullOrWhiteSpace(searchCriteria.Country))
            {
                sql.Append(" AND Country = @Country");
                parameters["Country"] = searchCriteria.Country;
            }

            if (!string.IsNullOrWhiteSpace(searchCriteria.AddressType))
            {
                sql.Append(" AND AddressType = @AddressType");
                parameters["AddressType"] = searchCriteria.AddressType;
            }

            sql.Append(" ORDER BY CreatedAt DESC");
            sql.Append(" OFFSET @Offset ROWS FETCH NEXT @Limit ROWS ONLY");

            parameters["Offset"] = options.Offset;
            parameters["Limit"] = options.Limit;

            try
            {
                return await QueryAsync<Address>(sql.ToString(), parameters);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to search addresses: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Get usage statistics for the address service
        /// </summary>
        public async Task<AddressStats> GetStatsAsync()
        {
            await InitializeAsync();

            try
            {
                var sql = $@"
                    SELECT 
                        (SELECT COUNT(*) FROM [{_options.AddressTableName}] WHERE IsActive = 1) AS TotalAddresses,
                        (SELECT COUNT(*) FROM [{_options.JoinTableName}] WHERE IsActive = 1) AS TotalRelationships";

                var result = await QueryAsync<AddressStats>(sql, new { });
                return result.FirstOrDefault() ?? new AddressStats();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to get stats: {ex.Message}", ex);
            }
        }

        // Helper methods for database operations
        private async Task EnsureConnectionOpenAsync()
        {
            if (_connection.State != ConnectionState.Open)
            {
                _connection.Open();
            }
        }

        private async Task<int> ExecuteNonQueryAsync(string sql, object? parameters = null, IDbTransaction? transaction = null)
        {
            using var command = _connection.CreateCommand();
            command.CommandText = sql;
            if (transaction != null) command.Transaction = transaction;

            if (parameters != null)
            {
                AddParameters(command, parameters);
            }

            return await Task.FromResult(command.ExecuteNonQuery());
        }

        private async Task<T> ExecuteScalarAsync<T>(string sql, object? parameters = null, IDbTransaction? transaction = null)
        {
            using var command = _connection.CreateCommand();
            command.CommandText = sql;
            if (transaction != null) command.Transaction = transaction;

            if (parameters != null)
            {
                AddParameters(command, parameters);
            }

            var result = await Task.FromResult(command.ExecuteScalar());
            return (T)Convert.ChangeType(result!, typeof(T));
        }

        private async Task<List<T>> QueryAsync<T>(string sql, object? parameters = null) where T : new()
        {
            using var command = _connection.CreateCommand();
            command.CommandText = sql;

            if (parameters != null)
            {
                AddParameters(command, parameters);
            }

            using var reader = await Task.FromResult(command.ExecuteReader());
            var results = new List<T>();

            while (reader.Read())
            {
                var item = new T();
                MapReaderToObject(reader, item);
                results.Add(item);
            }

            return results;
        }

        private void AddParameters(IDbCommand command, object parameters)
        {
            if (parameters is IDictionary<string, object> dict)
            {
                foreach (var kvp in dict)
                {
                    var parameter = command.CreateParameter();
                    parameter.ParameterName = $"@{kvp.Key}";
                    parameter.Value = kvp.Value ?? DBNull.Value;
                    command.Parameters.Add(parameter);
                }
            }
            else
            {
                var properties = parameters.GetType().GetProperties();
                foreach (var property in properties)
                {
                    var parameter = command.CreateParameter();
                    parameter.ParameterName = $"@{property.Name}";
                    parameter.Value = property.GetValue(parameters) ?? DBNull.Value;
                    command.Parameters.Add(parameter);
                }
            }
        }

        private void MapReaderToObject<T>(IDataReader reader, T obj)
        {
            var type = typeof(T);
            var properties = type.GetProperties();

            for (int i = 0; i < reader.FieldCount; i++)
            {
                var fieldName = reader.GetName(i);
                var property = properties.FirstOrDefault(p => 
                    string.Equals(p.Name, fieldName, StringComparison.OrdinalIgnoreCase));

                if (property != null && !reader.IsDBNull(i))
                {
                    var value = reader.GetValue(i);
                    if (property.PropertyType.IsGenericType && 
                        property.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        var underlyingType = Nullable.GetUnderlyingType(property.PropertyType);
                        value = Convert.ChangeType(value, underlyingType!);
                    }
                    else
                    {
                        value = Convert.ChangeType(value, property.PropertyType);
                    }
                    property.SetValue(obj, value);
                }
            }
        }
    }
}

-- # Address Service Package Structure

## Directory Layout

```
YourCompany.AddressService/
├── src/
│   └── YourCompany.AddressService/
│       ├── Models/
│       │   ├── Address.cs
│       │   ├── EntityAddress.cs
│       │   ├── CreateAddressRequest.cs
│       │   ├── UpdateAddressRequest.cs
│       │   ├── EntityInfo.cs
│       │   ├── SearchAddressCriteria.cs
│       │   ├── SearchOptions.cs
│       │   ├── GetAddressesOptions.cs
│       │   ├── LinkResult.cs
│       │   └── AddressStats.cs
│       ├── Services/
│       │   └── AddressService.cs
│       ├── Configuration/
│       │   └── AddressServiceOptions.cs
│       ├── Extensions/
│       │   └── ServiceCollectionExtensions.cs
│       └── YourCompany.AddressService.csproj
├── tests/
│   └── YourCompany.AddressService.Tests/
│       ├── AddressServiceTests.cs
│       └── YourCompany.AddressService.Tests.csproj
├── samples/
│   └── AddressService.Sample/
│       ├── Program.cs
│       └── AddressService.Sample.csproj
├── docs/
│   ├── README.md
│   └── API.md
├── .gitignore
├── Directory.Build.props
├── YourCompany.AddressService.sln
└── README.md
```

## Key Files

### 1. **YourCompany.AddressService.csproj**
```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <LangVersion>latest</LangVersion>
    <Nullable>enable</Nullable>
    <GeneratePackageOnBuild>true</GeneratePackageOnBuild>
    <PackageId>YourCompany.AddressService</PackageId>
    <PackageVersion>1.0.0</PackageVersion>
    <Authors>Your Company</Authors>
    <Description>Self-contained address service library for .NET applications</Description>
    <PackageTags>address;service;library;database</PackageTags>
    <RepositoryUrl>https://github.com/yourcompany/address-service</RepositoryUrl>
    <PackageLicenseExpression>MIT</PackageLicenseExpression>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.Extensions.DependencyInjection.Abstractions" Version="9.0.0" />
    <PackageReference Include="Microsoft.Extensions.Logging.Abstractions" Version="9.0.0" />
    <PackageReference Include="System.Data.Common" Version="4.3.0" />
  </ItemGroup>

</Project>
```

### 2. **ServiceCollectionExtensions.cs**
```csharp
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
```

### 3. **IAddressService.cs** (Interface)
```csharp
using YourCompany.AddressService.Models;

namespace YourCompany.AddressService.Services;

public interface IAddressService
{
    Task InitializeAsync();
    Task<Address> CreateAddressAsync(CreateAddressRequest addressData, EntityInfo? entityInfo = null);
    Task<LinkResult> LinkAddressToEntityAsync(int addressId, EntityInfo entityInfo);
    Task<List<EntityAddress>> GetEntityAddressesAsync(string entityType, int entityId, GetAddressesOptions? options = null);
    Task<Address?> GetAddressByIdAsync(int addressId);
    Task<Address?> UpdateAddressAsync(int addressId, UpdateAddressRequest updateData);
    Task<bool> DeactivateAddressAsync(int addressId);
    Task<bool> UnlinkAddressFromEntityAsync(int addressId, string entityType, int entityId, string? relationshipType = null);
    Task<List<Address>> SearchAddressesAsync(SearchAddressCriteria searchCriteria, SearchOptions? options = null);
    Task<AddressStats> GetStatsAsync();
}
```

### 4. **Directory.Build.props**
```xml
<Project>
  <PropertyGroup>
    <LangVersion>latest</LangVersion>
    <Nullable>enable</Nullable>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <WarningsAsErrors />
    <WarningsNotAsErrors>NU1701</WarningsNotAsErrors>
  </PropertyGroup>
</Project>
```

## Usage in Other Services

### 1. **Install the Package**
```bash
# If publishing to NuGet
dotnet add package YourCompany.AddressService

# If using local package
dotnet add reference ../path/to/YourCompany.AddressService/src/YourCompany.AddressService/YourCompany.AddressService.csproj
```

### 2. **Register in DI Container (Program.cs)**
```csharp
using YourCompany.AddressService.Extensions;
using Microsoft.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);

// Register with connection factory
builder.Services.AddAddressService(
    serviceProvider => new SqlConnection(builder.Configuration.GetConnectionString("DefaultConnection")),
    options =>
    {
        options.AddressTableName = "CustomAddresses";
        options.JoinTableName = "CustomEntityAddresses";
    });

var app = builder.Build();
```

### 3. **Use in Your Controllers/Services**
```csharp
using YourCompany.AddressService.Services;
using YourCompany.AddressService.Models;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IAddressService _addressService;
    
    public UsersController(IAddressService addressService)
    {
        _addressService = addressService;
    }
    
    [HttpPost("{userId}/addresses")]
    public async Task<IActionResult> CreateUserAddress(int userId, CreateAddressRequest request)
    {
        var address = await _addressService.CreateAddressAsync(
            request, 
            new EntityInfo 
            { 
                EntityType = "users", 
                EntityId = userId 
            });
            
        return Ok(new { AddressId = address.Id });
    }
}
```

## Benefits of This Structure

1. **Clean Separation**: Models, services, and configuration are properly separated
2. **DI Integration**: Easy to register and inject into your services
3. **Testable**: Interface-based design makes unit testing straightforward
4. **Configurable**: Options pattern allows customization per consuming service
5. **Package Ready**: Can be published to NuGet or used as a local reference
6. **Documentation**: Clear structure for docs and samples

## Alternative Distribution Methods

### 1. **NuGet Package** (Recommended)
- Build and publish to your private NuGet feed
- Easy versioning and updates
- Dependency management

### 2. **Git Submodule**
- Include as a submodule in consuming projects
- Direct source access
- Harder to version

### 3. **Local Package Reference**
- Reference the `.csproj` directly
- Good for development and testing
- Easy to modify and debug

--

# Address Service Package Structure

## Directory Layout

```
YourCompany.AddressService/
├── src/
│   └── YourCompany.AddressService/
│       ├── Models/
│       │   ├── Address.cs
│       │   ├── EntityAddress.cs
│       │   ├── CreateAddressRequest.cs
│       │   ├── UpdateAddressRequest.cs
│       │   ├── EntityInfo.cs
│       │   ├── SearchAddressCriteria.cs
│       │   ├── SearchOptions.cs
│       │   ├── GetAddressesOptions.cs
│       │   ├── LinkResult.cs
│       │   └── AddressStats.cs
│       ├── Services/
│       │   └── AddressService.cs
│       ├── Configuration/
│       │   └── AddressServiceOptions.cs
│       ├── Extensions/
│       │   └── ServiceCollectionExtensions.cs
│       └── YourCompany.AddressService.csproj
├── tests/
│   └── YourCompany.AddressService.Tests/
│       ├── AddressServiceTests.cs
│       └── YourCompany.AddressService.Tests.csproj
├── samples/
│   └── AddressService.Sample/
│       ├── Program.cs
│       └── AddressService.Sample.csproj
├── docs/
│   ├── README.md
│   └── API.md
├── .gitignore
├── Directory.Build.props
├── YourCompany.AddressService.sln
└── README.md
```

## Key Files

### 1. **YourCompany.AddressService.csproj**
```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <LangVersion>latest</LangVersion>
    <Nullable>enable</Nullable>
    <GeneratePackageOnBuild>true</GeneratePackageOnBuild>
    <PackageId>YourCompany.AddressService</PackageId>
    <PackageVersion>1.0.0</PackageVersion>
    <Authors>Your Company</Authors>
    <Description>Self-contained address service library for .NET applications</Description>
    <PackageTags>address;service;library;database</PackageTags>
    <RepositoryUrl>https://github.com/yourcompany/address-service</RepositoryUrl>
    <PackageLicenseExpression>MIT</PackageLicenseExpression>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.Extensions.DependencyInjection.Abstractions" Version="9.0.0" />
    <PackageReference Include="Microsoft.Extensions.Logging.Abstractions" Version="9.0.0" />
    <PackageReference Include="System.Data.Common" Version="4.3.0" />
  </ItemGroup>

</Project>
```

### 2. **ServiceCollectionExtensions.cs**
```csharp
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
```

### 3. **IAddressService.cs** (Interface)
```csharp
using YourCompany.AddressService.Models;

namespace YourCompany.AddressService.Services;

public interface IAddressService
{
    Task InitializeAsync();
    Task<Address> CreateAddressAsync(CreateAddressRequest addressData, EntityInfo? entityInfo = null);
    Task<LinkResult> LinkAddressToEntityAsync(int addressId, EntityInfo entityInfo);
    Task<List<EntityAddress>> GetEntityAddressesAsync(string entityType, int entityId, GetAddressesOptions? options = null);
    Task<Address?> GetAddressByIdAsync(int addressId);
    Task<Address?> UpdateAddressAsync(int addressId, UpdateAddressRequest updateData);
    Task<bool> DeactivateAddressAsync(int addressId);
    Task<bool> UnlinkAddressFromEntityAsync(int addressId, string entityType, int entityId, string? relationshipType = null);
    Task<List<Address>> SearchAddressesAsync(SearchAddressCriteria searchCriteria, SearchOptions? options = null);
    Task<AddressStats> GetStatsAsync();
}
```

### 4. **Directory.Build.props**
```xml
<Project>
  <PropertyGroup>
    <LangVersion>latest</LangVersion>
    <Nullable>enable</Nullable>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <WarningsAsErrors />
    <WarningsNotAsErrors>NU1701</WarningsNotAsErrors>
  </PropertyGroup>
</Project>
```

## Usage in Other Services

### 1. **Install the Package**
```bash
# If publishing to NuGet
dotnet add package YourCompany.AddressService

# If using local package
dotnet add reference ../path/to/YourCompany.AddressService/src/YourCompany.AddressService/YourCompany.AddressService.csproj
```

### 2. **Register in DI Container (Program.cs)**
```csharp
using YourCompany.AddressService.Extensions;
using Microsoft.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);

// Register with connection factory
builder.Services.AddAddressService(
    serviceProvider => new SqlConnection(builder.Configuration.GetConnectionString("DefaultConnection")),
    options =>
    {
        options.AddressTableName = "CustomAddresses";
        options.JoinTableName = "CustomEntityAddresses";
    });

var app = builder.Build();
```

### 3. **Use in Your Controllers/Services**
```csharp
using YourCompany.AddressService.Services;
using YourCompany.AddressService.Models;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IAddressService _addressService;
    
    public UsersController(IAddressService addressService)
    {
        _addressService = addressService;
    }
    
    [HttpPost("{userId}/addresses")]
    public async Task<IActionResult> CreateUserAddress(int userId, CreateAddressRequest request)
    {
        var address = await _addressService.CreateAddressAsync(
            request, 
            new EntityInfo 
            { 
                EntityType = "users", 
                EntityId = userId 
            });
            
        return Ok(new { AddressId = address.Id });
    }
}
```

## Cost-Effective Architecture Benefits

### **Single Database Advantages:**
1. **💰 Much Lower Costs**: One database instance vs multiple
2. **📈 Easy Scaling**: Scale up one database during peak season
3. **🔧 Simplified Management**: One connection pool, one backup strategy
4. **⚡ Better Performance**: Cross-domain queries possible if needed
5. **🛡️ Data Isolation**: Still logically separated by table prefixes

### **Seasonal Scaling Strategy:**
```csharp
// appsettings.Production.json for peak season
{
  "ConnectionStrings": {
    "SharedDatabase": "Server=high-performance-server;Database=MyAppDatabase;Connection Timeout=30;Max Pool Size=200;"
  }
}

// appsettings.json for off-season  
{
  "ConnectionStrings": {
    "SharedDatabase": "Server=basic-server;Database=MyAppDatabase;Connection Timeout=30;Max Pool Size=50;"
  }
}
```

### **Example Domain Structure (Cost-Optimized):**
```
YourCompany.Services/
├── UserService.Api/              # dotnet new webapi
├── OrderService.Api/             # dotnet new webapi  
├── CustomerService.Api/          # dotnet new webapi
├── ProductService.Api/           # dotnet new webapi
└── All connect to same database with different table prefixes
```

### **Database Growth Pattern:**
- **Off-Season**: Small database, basic hosting
- **Peak Season**: Scale up to premium database tier
- **Consistent**: All services still work identically
- **Future**: Easy to split databases later if needed

## Domain/Service Architecture Benefits

### **Perfect for Microservices/Domain-Driven Design:**

1. **Isolated Databases**: Each domain uses its own database
2. **Custom Table Names**: Avoid conflicts between services
3. **Domain-Specific Logic**: Each service handles its own address concerns
4. **Easy Integration**: Drop into any `dotnet new webapi` project
5. **Consistent Interface**: Same API across all domains


### **Example Domain Structure:**
```
YourCompany.Services/
├── UserService.Api/              # dotnet new webapi
│   ├── Controllers/
│   ├── Program.cs               # Registers AddressService with UserDB
│   └── appsettings.json         # UserDatabase connection
├── OrderService.Api/            # dotnet new webapi  
│   ├── Controllers/
│   ├── Program.cs               # Registers AddressService with OrderDB
│   └── appsettings.json         # OrderDatabase connection
├── CustomerService.Api/         # dotnet new webapi
│   ├── Controllers/
│   ├── Program.cs               # Registers AddressService with CustomerDB
│   └── appsettings.json         # CustomerDatabase connection
└── YourCompany.AddressService/  # Shared library
    └── src/...
```

## Alternative Distribution Methods

### 1. **NuGet Package** (Recommended)
- Build and publish to your private NuGet feed
- Easy versioning and updates
- Dependency management

### 2. **Git Submodule**
- Include as a submodule in consuming projects
- Direct source access
- Harder to version

### 3. **Local Package Reference**
- Reference the `.csproj` directly
- Good for development and testing
- Easy to modify and debug


### **Easy Domain-Specific Customization:**
```csharp
// In different domains, customize behavior
services.AddAddressService(
    connectionFactory,
    options =>
    {
        options.AddressTableName = "DomainSpecificAddresses";
        options.JoinTableName = "DomainEntityAddresses";
    });
```