using Microsoft.Extensions.Logging;
using Npgsql;
using System.Data;
using System.Data.Common;

using RoBrosRegistrantsService.Models;
using RoBrosRegistrantsService.Services;

namespace RoBrosRegistrantsService.Data;

public interface IRegistrantRepository
{
    Task<Guid> CreateRegistrantAsync(Registrant registrant);
    Task<Registrant> GetRegistrantAsync(Guid id);
    // Task<IEnumerable<Registrant>> GetAllRegistrantsAsync();
    // Task<Registrant> UpdateRegistrantAsync(Registrant registrant); // TODO: Break this into smaller parts and a whole, should it be named put
    Task<IEnumerable<Registrant>> SearchRegistrantsAsync(string queryStringParameters);
}

public class RegistrantRepository : IRegistrantRepository
{
    private readonly DatabaseService _databaseService;
    private readonly ILogger<RegistrantRepository> _logger;

    public RegistrantRepository(DatabaseService databaseService, ILogger<RegistrantRepository> logger)
    {
        _databaseService = databaseService;
        _logger = logger;
    }

    public async Task<Guid> CreateRegistrantAsync(Registrant registrant)
    {
        _logger.LogInformation("Inserting a new registrant into the database");
        registrant.AddressId = registrant.Address.Id;
        registrant.ChurchId = registrant.Church.Id;

        var sql = @"
            INSERT INTO registrant (
                Id, 
                GivenName,
                FamilyName, 
                ParticipantRole, 
                ChurchId, 
                YouthLeaderEmail, 
                YouthLeaderFirstName, 
                YouthLeaderLastName, 
                AddressId, 
                Mobile, 
                Email, 
                Birthday,
                Gender, 
                ShirtSize, 
                Price, 
                Paid, 
                Notes, 
                SubmissionDate, 
                IPAddress,
                CreatedAt,
                CreatedBy,
                UpdatedAt,
                UpdatedBy,
                Version
            ) VALUES (
                @Id,
                @GivenName, 
                @FamilyName, 
                @ParticipantRole, 
                @ChurchId, 
                @YouthLeaderEmail, 
                @YouthLeaderFirstName, 
                @YouthLeaderLastName, 
                @AddressId, 
                @Mobile, 
                @Email, 
                @Birthday, 
                @Gender, 
                @ShirtSize, 
                @Price, 
                @Paid, 
                @Notes, 
                @SubmissionDate, 
                @IPAddress,
                @CreatedAt,
                @CreatedBy,
                @UpdatedAt,
                @UpdatedBy,
                1
            )";

        int rowsAffected = await _databaseService.ExecuteAsync(sql, registrant);
        Console.WriteLine($"Rows affected: {rowsAffected}");
        
        if (rowsAffected > 0)
        {
            return registrant.Id;
        }

        throw new InvalidOperationException("Failed to add registrant.");
    }

    public async Task<Registrant> GetRegistrantAsync(Guid id)
    {
        _logger.LogInformation("Inserting a new registrant into the database");

        var sql = @"
            SELECT 
                Id, 
                GivenName,
                FamilyName, 
                ParticipantRole, 
                ChurchId, 
                YouthLeaderEmail, 
                YouthLeaderFirstName, 
                YouthLeaderLastName, 
                AddressId, 
                Mobile, 
                Email, 
                Birthday,
                Gender,
                ShirtSize
            FROM registrant 
            WHERE Id = @id";

        // using var reader = await _databaseService.QueryAsync<Registrant>(sql, id);
        // if (await reader.ReadAsync())
        // {
        //     return MapRegistrantFromReader(reader);
        // }
        var registrant = await _databaseService.QuerySingleAsync<Registrant>(sql, new { id });
        if (registrant != null)
        {
            return registrant;
        }

        throw new InvalidOperationException("Failed to add registrant.");
    }

    public async Task<List<Registrant>> GetAllRegistrantsAsync()
    {
        var sql = "SELECT * FROM registrant";
        
        // using var reader = await _databaseService.QueryAsync(sql);
        // if (await reader.ReadAsync())
        // {
        //     return MapRegistrantFromReader(reader);
        // }

        var registrants = (await _databaseService.QueryAsync<Registrant>(sql)).ToList();
        if (registrants != null)
        {
            return registrants;
        }

        throw new InvalidOperationException("Failed to registrants");
    }

    public async Task<Registrant> UpdateRegistrantAsync(Registrant registrant)
    {
         var sql = @"
            UPDATE registrant (
                Id, 
                GivenName,
                FamilyName, 
                ParticipantRole, 
                ChurchId, 
                YouthLeaderEmail, 
                YouthLeaderFirstName, 
                YouthLeaderLastName, 
                AddressId, 
                Mobile, 
                Email, 
                Birthday,
                Gender, 
                ShirtSize, 
                Price, 
                Paid, 
                Notes, 
                SubmissionDate, 
                IPAddress,
                CreatedAt,
                CreatedBy,
                UpdatedAt,
                UpdatedBy
            ) VALUES (
                @Id,
                @GivenName, 
                @FamilyName, 
                @ParticipantRole, 
                @Church, 
                @YouthLeaderEmail, 
                @YouthLeaderFirstName, 
                @YouthLeaderLastName, 
                @Address, 
                @Mobile, 
                @Email, 
                @Birthday, 
                @Gender, 
                @ShirtSize, 
                @Price, 
                @Paid, 
                @Notes, 
                @SubmissionDate, 
                @IPAddress,
                @CreatedAt,
                @CreatedBy,
                @UpdatedAt,
                @UpdatedBy
            )
            WHERE Id = @Id
            RETURNING *";

        // int rowsAffected = await _databaseService.ExecuteAsync(sql, registrant);
        // Console.WriteLine($"Rows affected: {rowsAffected}");
        
        // if (rowsAffected > 0)
        // {
        //     return MapRegistrantFromReader(registrant);
        // }

        var updatedRegistrant = await _databaseService.QuerySingleAsync<Registrant>(sql, registrant);
        if (updatedRegistrant != null)
        {
            return updatedRegistrant;
        }

        throw new InvalidOperationException("Failed to update registrant.");
    }

    public async Task<IEnumerable<Registrant>> SearchRegistrantsAsync(string queryStringParameters)
    {
        var sql = @"SELECT * FROM registrants 
            WHERE GivenName iLike '%@queryStringParameters%'
            OR FamilyName iLike '%@queryStringParameters%'
            OR ChurchId iLike '%@queryStringParameters%'
            ";
        // Need to figure out the querystring and defined it
        // using var reader = await _databaseService.QueryAsync<IEnumerable<Registrant>>(sql, new { queryStringParameters });
        // if (await reader.ReadAsync())
        // {
        //     return [MapRegistrantFromReader(reader)];
        // }
        var registrants = await _databaseService.QueryAsync<Registrant>(sql, queryStringParameters);
        if (registrants != null)
        {
            return registrants;
        }

        return new List<Registrant> ();
    }

    private static Registrant MapRegistrantFromReader(DbDataReader reader)
    {
        Address registrantAddress = new Address
        {
            StreetAddress1 = reader.GetString("StreetAddress1"),
            StreetAddress2 = reader.GetString("StreetAddress2"),
            Locality = reader.GetString("Locality"),
            AdministrativeAreaLevel = reader.GetString("AdministrativeAreaLevel"),
            PostalCode = reader.GetInt32("PostalCode"),
            Country = reader.GetString("Country")
        }; 

        Church registrantChurch = new Church
        {
            Id = reader.GetGuid("ChurchId"),
            Name = reader.GetString("ChurchName"),
            Address = new Address
            {
                StreetAddress1 = reader.GetString("ChurchStreetAddress1"),
                StreetAddress2 = reader.GetString("ChurchStreetAddress2"),
                Locality = reader.GetString("ChurchLocality"),
                AdministrativeAreaLevel = reader.GetString("ChurchAdministrativeAreaLevel"),
                PostalCode = reader.GetInt32("ChurchPostalCode"),
                Country = reader.GetString("ChurchCountry")
            }
        };

        return new Registrant
        {
            Id = reader.GetGuid("id"),
            GivenName = reader.GetString("GivenName"),
            FamilyName = reader.GetString("FamilyName"), 
            ParticipantRole = reader.GetString("ParticipantRole"), 
            YouthLeaderEmail = reader.GetString("YouthLeaderEmail"), 
            YouthLeaderFirstName = reader.GetString("YouthLeaderFirstName"), 
            YouthLeaderLastName = reader.GetString("YouthLeaderLastName"), 
            Mobile = reader.GetString("Mobile"), 
            Email = reader.GetString("Email"), 
            Birthday = reader.GetDateTime("Birthday"),
            Gender = reader.GetString("Gender"),
            ShirtSize = reader.GetString("ShirtSize"),
            // These should return as whole but be stored as ID
            Address = registrantAddress, 
            Church = registrantChurch, 
            // Remove from Response
            Paid = reader.GetBoolean("Paid"),
            SubmissionDate = reader.GetDateTime("SubmissionDate"),
            IPAddress = reader.GetString("IPAddress")
        };
    }
}