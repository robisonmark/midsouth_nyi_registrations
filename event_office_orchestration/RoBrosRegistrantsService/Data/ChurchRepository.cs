using RoBrosRegistrantsService.Models;
using RoBrosRegistrantsService.Services;

namespace RoBrosRegistrantsService.Data;

public class ChurchRepository
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
            INSERT INTO churches (id, name, created_by, created_at, updated_by, updated_at, version)
            VALUES (@Id, @Name, @CreatedBy, NOW(), @UpdatedBy, NOW(), 1)
            RETURNING id;";

        var parameters = new { Id = id, Name = church.Name, CreatedBy = church.createdBy ?? "System", UpdatedBy = church.updatedBy ?? "System" };

        // QuerySingleAsync will return the GUID from RETURNING
        var returned = await _databaseService.QuerySingleAsync<Guid>(sql, parameters);
        return returned;
    }

    public async Task<Church?> GetByNameAsync(string name)
    {
        var sql = "SELECT id, name, address_id, created_at, created_by, updated_at, updated_by, version FROM churches WHERE LOWER(Name) = LOWER(@Name) LIMIT 1";
        return await _databaseService.QuerySingleAsync<Church>(sql, new { Name = name });
    }

    public async Task<Church?> GetByIdAsync(Guid id)
    {
        var sql = "SELECT Id, Name, AddressId, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, Version FROM churches WHERE Id = @Id";
        return await _databaseService.QuerySingleAsync<Church>(sql, new { Id = id });
    }
}
