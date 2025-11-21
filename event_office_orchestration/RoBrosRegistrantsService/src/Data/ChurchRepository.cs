using System.Data;
using System.Data.Common;
using System.Reflection.Metadata.Ecma335;
using EventOfficeApi.RoBrosAddressesService.Models;
using RoBrosRegistrantsService.Models;
using RoBrosRegistrantsService.Services;

namespace RoBrosRegistrantsService.Data;

public interface IChurchRepository
{
    Task<Guid> CreateAsync(Church church);

    Task<Church?> GetByNameAsync(string name);

    Task<Church?> GetByIdAsync(Guid id);
}

public class ChurchRepository : IChurchRepository
{
    private readonly DatabaseService _databaseService;

    public ChurchRepository(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    public async Task<Guid> CreateAsync(Church church)
    {
        var id = church.Id ?? Guid.NewGuid();
        church.Id = id;

        var sql = @"
            INSERT INTO churches (id, name, address_id, created_by, created_at, updated_by, updated_at, version)
            OUTPUT INSERTED.id
            VALUES (@Id, @Name, @AddressId, @CreatedBy, current_timestamp, @UpdatedBy, current_timestamp, 1);";

        var parameters = new
        {
            Id = id,
            AddressId = church.AddressId,
            Name = church.Name,
            CreatedBy = church.createdBy ?? "System",
            UpdatedBy = church.updatedBy ?? "System"
        };

        // QuerySingleAsync will return the GUID from RETURNING
        var returned = await _databaseService.QuerySingleAsync<Guid>(sql, parameters);
        return returned;
    }

    public async Task<Church?> GetByNameAsync(string name)
    {
        var sql = @"SELECT TOP 1
                c.id,
                c.name,
                c.address_id,
                c.created_by,
                c.created_at,
                c.updated_by,
                c.updated_at,
                a.street_address_1 AS street_address_1,
                a.street_address_2 AS street_address_2,
                a.city AS city,
                a.state AS state,
                a.postal_code AS postal_code,
                a.country AS country
            FROM churches c
            LEFT JOIN addresses a ON c.address_id = a.id
            WHERE LOWER(c.name) = LOWER(@Name);";


        var row = await _databaseService.QuerySingleAsync<dynamic>(sql, new { Name = name });
        if (row == null)
        {
            return null;
        }

        CreateAddressRequest? churchAddress = null;
        if (row.address_id != null)
        {
            churchAddress = new CreateAddressRequest
            {
                StreetAddress1 = row.street_address_1 ?? string.Empty,
                StreetAddress2 = row.street_address_2 ?? string.Empty,
                City = row.city ?? string.Empty,
                State = row.state ?? string.Empty,
                PostalCode = row.postal_code ?? string.Empty,
                Country = row.country ?? string.Empty
            };
        }

        var response = new Church
        {
            Id = row.id,
            Name = row.name,
            AddressId = row.address_id,
            Address = churchAddress,
            createdBy = row.created_by,
            createdAt = row.created_at,
            updatedBy = row.updated_by,
            updatedAt = row.updated_at
        };

        return response;
    }

    public async Task<Church?> GetByIdAsync(Guid id)
    {
        var sql = @"SELECT 
                churches.id
                , name
                , addresses.street_address_1
                , addresses.street_address_2
                , city
                , state
                , postal_code
                , churches.created_at
                , churches.created_by
                , churches.updated_at
                , churches.updated_by
                , churches.version 
            FROM churches
                LEFT JOIN addresses on churches.address_id = addresses.id
            WHERE churches.id =  @Id";
        var row = await _databaseService.QuerySingleAsync<dynamic>(sql, new { Id = id });
        if (row == null)
        {
            throw new InvalidOperationException("Failed to get registrant.");
        }

        Console.WriteLine(row);
        CreateAddressRequest churchAddress = new CreateAddressRequest
        {
            StreetAddress1 = row.street_address_1 ?? string.Empty,
            StreetAddress2 = row.street_address_2 ?? string.Empty,
            City = row.city ?? string.Empty,
            State = row.state ?? string.Empty,
            PostalCode = row.postal_code ?? string.Empty,
        };

        Church response = new Church
        {
            Id = row.id,
            Name = row.name,
            Address = churchAddress,
            createdAt = row.created_at,
            createdBy = row.created_by,
            updatedAt = row.updated_at,
            updatedBy = row.updated_by,         
        };

        return response;
    }
}
