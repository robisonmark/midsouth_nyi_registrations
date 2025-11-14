using EventOfficeApi.RoBrosAddressesService.Interfaces;
using EventOfficeApi.RoBrosAddressesService.Models;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Data;
using System.Data.Common;

namespace EventOfficeApi.RoBrosAddressesService.Data;

public class AddressRepository : IAddressRepository
{
    // private readonly string _connectionString;
    private readonly ISqlProvider _sqlProvider;

    private readonly NpgsqlDataSource _dataSource;
    
    private readonly ILogger<AddressRepository> _logger;

    // public AddressRepository(string connectionString, ISqlProvider sqlProvider, ILogger<AddressRepository> logger)
    public AddressRepository(NpgsqlDataSource dataSource, ISqlProvider sqlProvider, ILogger<AddressRepository> logger)
    {
        _dataSource = dataSource;
        // _connectionString = connectionString;
        _sqlProvider = sqlProvider;
        _logger = logger;
    }
    

    public async Task<Address?> GetByIdAsync(Guid id)
    {
        await using var connection = await _dataSource.OpenConnectionAsync();
        using var command = new NpgsqlCommand(_sqlProvider.GetSelectAddressByIdQuery(), connection);

        command.Parameters.AddWithValue("Id", id);


        using var reader = await command.ExecuteReaderAsync();
        return await MapAddressWithMappings(reader);
    }

    public async Task<IEnumerable<Address>> GetByEntityAsync(Guid entityId, string entityType)
    {
        // using var connection = new NpgsqlConnection(_connectionString);
        // await connection.OpenAsync();
        await using var connection = await _dataSource.OpenConnectionAsync();

        using var command = new NpgsqlCommand(_sqlProvider.GetSelectAddressByEntityQuery());
        command.Parameters.AddWithValue("EntityId", entityId);
        command.Parameters.AddWithValue("EntityType", entityType);

        using var reader = await command.ExecuteReaderAsync();
        return await MapAddressesWithMappings(reader);
    }

    public async Task<Guid> CreateAsync(CreateAddressRequest request)
    {
        // Should this be a try{} catch{} block?
        var address = new Address
        {
            StreetAddress1 = request.StreetAddress1,
            StreetAddress2 = request.StreetAddress2,
            City = request.City,
            State = request.State,
            PostalCode = request.PostalCode,
            Country = request.Country,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "Mark",
            UpdatedAt = DateTime.UtcNow,
            UpdatedBy = "Mark"
        };

        // var exists = await CheckExistsBeforeCreate(address);
        // if (exists)
        // {
        //     _logger.LogInformation("Address already exists");
        //     throw new FileNotFoundException("Address Already Exists");
        // }

        // await using var connection = new NpgsqlConnection(_connectionString);
            // try
            // {
            //     if (connection.State != System.Data.ConnectionState.Open)
            //         Console.WriteLine("Connection: " + connection.ConnectionString);
            // await connection.OpenAsync();
            // connection.Open();
            // }
            // catch (Exception ex)
            // {
            //     Console.WriteLine("test " + ex);
            // }

        await using var connection = await _dataSource.OpenConnectionAsync();
        using var command = new NpgsqlCommand(_sqlProvider.GetCreateAddressQuery(), connection);

        AddAddressParameters(command, address);


        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return reader.GetGuid("id");
        }

        throw new InvalidOperationException("Failed to create address");
    }

    public async Task<Address?> UpdateAsync(Guid id, UpdateAddressRequest request)
    {
        // using var connection = new NpgsqlConnection(_connectionString);
        // await connection.OpenAsync();
        await using var connection = await _dataSource.OpenConnectionAsync();

        using var command = new NpgsqlCommand(_sqlProvider.GetUpdateAddressQuery());
        command.Parameters.AddWithValue("Id", id);
        command.Parameters.AddWithValue("StreetAddress1", (object?)request.StreetAddress1 ?? DBNull.Value);
        command.Parameters.AddWithValue("StreetAddress2", (object?)request.StreetAddress2 ?? DBNull.Value);
        command.Parameters.AddWithValue("City", (object?)request.City ?? DBNull.Value);
        command.Parameters.AddWithValue("State", (object?)request.State ?? DBNull.Value);
        command.Parameters.AddWithValue("PostalCode", (object?)request.PostalCode ?? DBNull.Value);
        command.Parameters.AddWithValue("Country", (object?)request.Country ?? DBNull.Value);
        command.Parameters.AddWithValue("UpdatedAt", DateTime.UtcNow);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapAddressFromReader(reader);
        }

        return null;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        // using var connection = new NpgsqlConnection(_connectionString);
        // await connection.OpenAsync();
        await using var connection = await _dataSource.OpenConnectionAsync();

        using var command = new NpgsqlCommand(_sqlProvider.GetDeleteAddressQuery());
        command.Parameters.AddWithValue("Id", id);

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<AddressEntityMapping> MapToEntityAsync(Guid addressId, Guid entityId, string entityType, string? addressType = null)
    {
        var mapping = new AddressEntityMapping
        {
            Id = Guid.NewGuid(),
            AddressId = addressId,
            EntityId = entityId,
            EntityType = entityType,
            AddressType = addressType,
            CreatedAt = DateTime.UtcNow
        };

        // using var connection = new NpgsqlConnection(_connectionString);
        // await connection.OpenAsync();
        await using var connection = await _dataSource.OpenConnectionAsync();

        using var command = new NpgsqlCommand(_sqlProvider.GetCreateMappingQuery());
        command.Parameters.AddWithValue("Id", mapping.Id);
        command.Parameters.AddWithValue("AddressId", mapping.AddressId);
        command.Parameters.AddWithValue("EntityId", mapping.EntityId);
        command.Parameters.AddWithValue("EntityType", mapping.EntityType);
        command.Parameters.AddWithValue("AddressType", (object?)mapping.AddressType ?? DBNull.Value);
        command.Parameters.AddWithValue("CreatedAt", mapping.CreatedAt);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapMappingFromReader(reader);
        }

        throw new InvalidOperationException("Failed to create address mapping");
    }

    public async Task<bool> UnmapFromEntityAsync(Guid addressId, Guid entityId, string entityType)
    {
        // using var connection = new NpgsqlConnection(_connectionString);
        // await connection.OpenAsync();
        await using var connection = await _dataSource.OpenConnectionAsync();

        using var command = new NpgsqlCommand(_sqlProvider.GetDeleteMappingQuery());
        command.Parameters.AddWithValue("AddressId", addressId);
        command.Parameters.AddWithValue("EntityId", entityId);
        command.Parameters.AddWithValue("EntityType", entityType);

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<IEnumerable<AddressEntityMapping>> GetMappingsByEntityAsync(Guid entityId, string entityType)
    {
        // using var connection = new NpgsqlConnection(_connectionString);
        // await connection.OpenAsync();
        await using var connection = await _dataSource.OpenConnectionAsync();

        using var command = new NpgsqlCommand(_sqlProvider.GetSelectMappingsByEntityQuery());
        command.Parameters.AddWithValue("EntityId", entityId);
        command.Parameters.AddWithValue("EntityType", entityType);

        using var reader = await command.ExecuteReaderAsync();
        var mappings = new List<AddressEntityMapping>();

        while (await reader.ReadAsync())
        {
            mappings.Add(MapMappingFromReader(reader));
        }

        return mappings;
    }

    public async Task<IEnumerable<Address>> SearchAsync(string? city = null, string? state = null, string? postalCode = null)
    {
        // using var connection = new NpgsqlConnection(_connectionString);
        // await connection.OpenAsync();
        await using var connection = await _dataSource.OpenConnectionAsync();

        using var command = new NpgsqlCommand(_sqlProvider.GetSearchAddressesQuery());
        command.Parameters.AddWithValue("City", (object?)city ?? DBNull.Value);
        command.Parameters.AddWithValue("State", (object?)state ?? DBNull.Value);
        command.Parameters.AddWithValue("PostalCode", (object?)postalCode ?? DBNull.Value);

        using var reader = await command.ExecuteReaderAsync();
        return await MapAddressesWithMappings(reader);
    }

    public async Task<bool> CheckExistsBeforeCreate(Address address)
    {
        // TODO: see about using existing connection?
        await using var connection = await _dataSource.OpenConnectionAsync();

        using var command = new NpgsqlCommand(_sqlProvider.GetAddressExistsQuery(), connection);
        command.Parameters.AddWithValue("Id", address.Id);
        command.Parameters.AddWithValue("StreetAddress1", address.StreetAddress1);
        command.Parameters.AddWithValue("City", address.City);
        command.Parameters.AddWithValue("PostalCode", address.PostalCode);

        var result = await command.ExecuteScalarAsync();
        return result is bool exists && exists;
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        // using var connection = new NpgsqlConnection(_connectionString);
        // await connection.OpenAsync();
        await using var connection = await _dataSource.OpenConnectionAsync();

        using var command = new NpgsqlCommand(_sqlProvider.GetAddressExistsQuery());
        command.Parameters.AddWithValue("Id", id);

        var result = await command.ExecuteScalarAsync();
        return result is bool exists && exists;
    }

    private static void AddAddressParameters(NpgsqlCommand command, Address address)
    {
        command.Parameters.AddWithValue("Id", address.Id);
        command.Parameters.AddWithValue("StreetAddress1", address.StreetAddress1);
        command.Parameters.AddWithValue("StreetAddress2", (object?)address.StreetAddress2 ?? DBNull.Value);
        command.Parameters.AddWithValue("City", address.City);
        command.Parameters.AddWithValue("State", address.State);
        command.Parameters.AddWithValue("PostalCode", address.PostalCode);
        command.Parameters.AddWithValue("Country", address.Country);
        command.Parameters.AddWithValue("CreatedAt", address.CreatedAt);
        command.Parameters.AddWithValue("CreatedBy", address.CreatedBy);
        command.Parameters.AddWithValue("UpdatedAt", address.UpdatedAt);
        command.Parameters.AddWithValue("UpdatedBy", address.UpdatedBy);
    }

    private static Address MapAddressFromReader(DbDataReader reader)
    {
        return new Address
        {
            Id = reader.GetGuid("id"),
            StreetAddress1 = reader.GetString("street_address_1"),
            StreetAddress2 = reader.IsDBNull("street_address_2") ? null : reader.GetString("street_address_2"),
            City = reader.GetString("locality"),
            State = reader.GetString("administrative_area_level"),
            PostalCode = reader.GetString("postal_code"),
            Country = reader.GetString("country"),
            CreatedAt = reader.GetDateTime("created_at"),
            CreatedBy = reader.GetString("created_by"),
            UpdatedAt = reader.GetDateTime("updated_at"),
            UpdatedBy = reader.GetString("updated_by")
        };
    }

    private static AddressEntityMapping MapMappingFromReader(DbDataReader reader)
    {
        return new AddressEntityMapping
        {
            Id = reader.GetGuid("id"),
            AddressId = reader.GetGuid("address_id"),
            EntityId = reader.GetGuid("entity_id"),
            EntityType = reader.GetString("entity_type"),
            AddressType = reader.IsDBNull("address_type") ? null : reader.GetString("address_type"),
            CreatedAt = reader.GetDateTime("created_at")
        };
    }

    private static async Task<Address?> MapAddressWithMappings(DbDataReader reader)
    {
        var addresses = await MapAddressesWithMappings(reader);
        return addresses.FirstOrDefault();
    }

    private static async Task<List<Address>> MapAddressesWithMappings(DbDataReader reader)
    {
        var addressDict = new Dictionary<Guid, Address>();

        while (await reader.ReadAsync())
        {
            var addressId = reader.GetGuid("id");
            
            if (!addressDict.TryGetValue(addressId, out var address))
            {
                address = MapAddressFromReader(reader);
                addressDict[addressId] = address;
            }

            // Check if there's a mapping
            if (!reader.IsDBNull("mapping_id"))
            {
                var mapping = new AddressEntityMapping
                {
                    Id = reader.GetGuid("mapping_id"),
                    AddressId = addressId,
                    EntityId = reader.GetGuid("entity_id"),
                    EntityType = reader.GetString("entity_type"),
                    AddressType = reader.IsDBNull("address_type") ? null : reader.GetString("address_type"),
                    CreatedAt = reader.GetDateTime("mapping_created_at")
                };

                address.EntityMappings.Add(mapping);
            }
        }

        return addressDict.Values.ToList();
    }
}