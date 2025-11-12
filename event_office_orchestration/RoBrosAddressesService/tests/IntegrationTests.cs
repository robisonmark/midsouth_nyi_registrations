using EventOfficeApi.RoBrosAddressesService.Data;
using EventOfficeApi.RoBrosAddressesService.Models;
using Microsoft.Extensions.Logging;
using Testcontainers.PostgreSql;
using Xunit;
using FluentAssertions;
using Npgsql;

namespace Tests;

public class IntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:15")
        .WithDatabase("addresstest")
        .WithUsername("testuser")
        .WithPassword("testpass")
        .Build();

    private AddressRepository _repository = null!;
    private string _connectionString = null!;

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        _connectionString = _postgres.GetConnectionString();
        
        // Setup database schema
        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        
        var sqlProvider = new PostgreSqlProvider();
        using var command = new NpgsqlCommand(sqlProvider.GetCreateTablesScript(), connection);
        await command.ExecuteNonQueryAsync();

        // Initialize repository using the new NpgsqlDataSource-based constructor
        var logger = LoggerFactory.Create(builder => builder.AddConsole())
            .CreateLogger<AddressRepository>();

        var dataSource = NpgsqlDataSource.Create(_connectionString);
        _repository = new AddressRepository(dataSource, sqlProvider, logger);
    }

    public async Task DisposeAsync()
    {
        await _postgres.DisposeAsync();
    }

    [Fact]
    public async Task CreateAddress_ShouldReturnValidAddress()
    {
        // Arrange
        var request = new CreateAddressRequest
        {
            StreetAddress1 = "123 Main St",
            StreetAddress2 = "Apt 4B",
            City = "Nashville",
            State = "TN",
            PostalCode = "37203",
            Country = "USA"
        };

        // Act
        var id = await _repository.CreateAsync(request);

        // Assert
        id.Should().NotBe(Guid.Empty);

        var result = await _repository.GetByIdAsync(id);
        result.Should().NotBeNull();
        result!.Id.Should().Be(id);
        result.StreetAddress1.Should().Be(request.StreetAddress1);
        result.City.Should().Be(request.City);
        result.State.Should().Be(request.State);
        result.PostalCode.Should().Be(request.PostalCode);
        result.Country.Should().Be(request.Country);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ShouldReturnAddress()
    {
        // Arrange
        var request = new CreateAddressRequest
        {
            StreetAddress1 = "456 Oak Ave",
            City = "Memphis",
            State = "TN",
            PostalCode = "38103",
            Country = "USA"
        };
        Guid created = await _repository.CreateAsync(request);

        // Act
        var result = await _repository.GetByIdAsync(created);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(created);
        result.StreetAddress1.Should().Be(request.StreetAddress1);
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        // Act
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task MapToEntityAsync_ShouldCreateMapping()
    {
        // Arrange
        var address = await CreateTestAddress();
        var entityId = Guid.NewGuid();
        var entityType = "Customer";
        var addressType = "billing";

        // Act
        var mapping = await _repository.MapToEntityAsync(address, entityId, entityType, addressType);

        // Assert
        mapping.Should().NotBeNull();
        mapping.AddressId.Should().Be(address);
        mapping.EntityId.Should().Be(entityId);
        mapping.EntityType.Should().Be(entityType);
        mapping.AddressType.Should().Be(addressType);
    }

    [Fact]
    public async Task GetByEntityAsync_ShouldReturnMappedAddresses()
    {
        // Arrange
        var address1 = await CreateTestAddress();
        var address2 = await CreateTestAddress();
        var entityId = Guid.NewGuid();
        var entityType = "Order";

        await _repository.MapToEntityAsync(address1, entityId, entityType, "shipping");
        await _repository.MapToEntityAsync(address2, entityId, entityType, "billing");

        // Act
        var result = await _repository.GetByEntityAsync(entityId, entityType);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(a => a.Id == address1);
        result.Should().Contain(a => a.Id == address2);
    }

    [Fact]
    public async Task UpdateAsync_ShouldModifyAddress()
    {
        // Arrange
        var address = await CreateTestAddress();
        var updateRequest = new UpdateAddressRequest
        {
            StreetAddress1 = "Updated Street",
            City = "Updated City"
        };

        // Act
        var result = await _repository.UpdateAsync(address, updateRequest);

        // Assert
        result.Should().NotBeNull();
        result!.StreetAddress1.Should().Be(updateRequest.StreetAddress1);
        result.City.Should().Be(updateRequest.City);
        // result.State.Should().Be(address.State); // Unchanged
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveAddress()
    {
        // Arrange
        var address = await CreateTestAddress();

        // Act
        var deleted = await _repository.DeleteAsync(address);

        // Assert
        deleted.Should().BeTrue();

        var retrieved = await _repository.GetByIdAsync(address);
        retrieved.Should().BeNull();
    }

    [Fact]
    public async Task SearchAsync_ShouldFindMatchingAddresses()
    {
        // Arrange
        await CreateTestAddress("Nashville", "TN");
        await CreateTestAddress("Memphis", "TN");
        await CreateTestAddress("Atlanta", "GA");

        // Act
        var tnResults = await _repository.SearchAsync(state: "TN");
        var nashvilleResults = await _repository.SearchAsync(city: "Nashville");

        // Assert
        tnResults.Should().HaveCount(2);
        nashvilleResults.Should().HaveCount(1);
    }

    private async Task<Guid> CreateTestAddress(string city = "TestCity", string state = "TS")
    {
        var request = new CreateAddressRequest
        {
            StreetAddress1 = "123 Test St",
            City = city,
            State = state,
            PostalCode = "12345",
            Country = "USA"
        };
        return await _repository.CreateAsync(request);
    }
}