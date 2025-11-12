using Microsoft.Extensions.Logging;
using Npgsql;
using System.Data;
using System.Data.Common;

using EventOfficeApi.RoBrosAddressesService.Models;

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

        Guid registrantId = Guid.NewGuid();
        registrant.Id = registrantId;

        var registrantSQL = @"
            INSERT INTO registrants (
                id,
                given_name,
                family_name, 
                participant_role, 
                church_id, 
                youth_leader_email, 
                youth_leader_first_name, 
                youth_leader_last_name, 
                address_id, 
                cell_number, 
                email, 
                birthday,
                gender, 
                shirt_size, 
                price,
                paid,
                notes, 
                submission_date, 
                ip_address,
                created_at,
                created_by,
                updated_at,
                updated_by,
                version
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
            );";
        

        
        int rowsAffected = await _databaseService.ExecuteAsync(registrantSQL, registrant);
        Console.WriteLine($"Rows affected: {rowsAffected}");
        
            if (rowsAffected > 0)
            {
                // If the registrant is a Competitor (inherits from Registrant), insert
                // competitor-specific details into the student table. We detect this via
                // runtime type and cast so we can pass the full Competitor object to the
                // database helper which expects property names matching the SQL params.
                if (registrant is RoBrosRegistrantsService.Models.Competitor competitor)
                {
                    // Ensure the child object has the registrant id populated
                    competitor.RegistrantId = registrant.Id;

                    var competitorSQL = @"
                    INSERT INTO student (
                        registrant_id,
                        district,
                        competition_status,
                        medical_conditions,
                        dietary_restrictions,
                        allergies,
                        food_allergies,
                        medications,
                        guardian_first_name,
                        guardian_last_name,
                        guardian_home_phone,
                        guardian_work_phone,
                        guardian_contact_phone,
                        insurance_company,
                        policy_id,
                        quizzing,
                        art_categories,
                        creative_ministries_categories,
                        creative_writing_categories,
                        speech_categories,
                        vocal_music_categories,
                        instrumental_music_categories,
                        individual_sport_categories,
                        team_sport_categories,
                        attending_tnt_at_tnu
                    ) VALUES (
                        @RegistrantId,
                        @District,
                        @CompetitionStatus,
                        @MedicalConditions,
                        @DietaryRestrictions,
                        @Allergies,
                        @FoodAllergies,
                        @Medications,
                        @GuardianFirstName,
                        @GuardianLastName,
                        @GuardianHomePhone,
                        @GuardianWorkPhone,
                        @GuardianContactPhone,
                        @InsuranceCompany,
                        @PolicyId,
                        @Quizzing,
                        @ArtCategories,
                        @CreativeMinistriesCategories,
                        @CreativeWritingCategories,
                        @SpeechCategories,
                        @VocalMusicCategories,
                        @InstrumentalMusicCategories,
                        @IndividualSportCategories,
                        @TeamSportCategories,
                        @AttendingTNTatTNU
                    );";

                    int competitorRowsAffected = await _databaseService.ExecuteAsync(competitorSQL, competitor);
                    Console.WriteLine($"Competitor Rows affected: {competitorRowsAffected}");
                }

                return registrant.Id;
            }

        throw new InvalidOperationException("Failed to add registrant.");
    }

    public async Task<Registrant> GetRegistrantAsync(Guid id)
    {
        _logger.LogInformation("Inserting a new registrant into the database");

        var sql = @"
            SELECT 
                id, 
                given_name,
                family_name, 
                participant_role, 
                church_id, 
                youth_leader_email, 
                youth_leader_first_name, 
                youth_leader_last_name, 
                address_id, 
                cell_number, 
                email, 
                birthday,
                gender,
                shirt_size
            FROM registrants 
            WHERE id = @id";

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

    public async Task<IEnumerable<Registrant>> SearchRegistrantsAsync(string searchParameters)
    {
        IEnumerable<Registrant> return_value = new List<Registrant>();
        // Use a parameterized pattern for ILIKE. Passing the % wildcard in the SQL string
        // around the parameter (e.g. '%@p%') does NOT substitute the parameter value.
        // Two safe options are: (1) build the pattern in .NET and pass it as a parameter,
        // or (2) use SQL concatenation: column ILIKE '%' || @p || '%'.
         var sql = @"SELECT
            registrants.id AS Id,
            registrants.given_name AS GivenName,
            registrants.family_name AS FamilyName,
            registrants.participant_role AS ParticipantRole,
            registrants.church_id AS ChurchId,
            registrants.youth_leader_email AS YouthLeaderEmail,
            registrants.youth_leader_first_name AS YouthLeaderFirstName,
            registrants.youth_leader_last_name AS YouthLeaderLastName,
            registrants.address_id AS AddressId,
            registrants.cell_number AS Mobile,
            registrants.email AS Email,
            registrants.birthday AS Birthday,
            registrants.gender AS Gender,
            registrants.shirt_size AS ShirtSize,
            registrants.price AS Price,
            registrants.paid AS Paid,
            registrants.notes AS Notes,
            registrants.submission_date AS SubmissionDate,
            registrants.ip_address AS IPAddress
        FROM registrants
        INNER JOIN churches ON registrants.church_id = churches.id
        WHERE registrants.given_name ILIKE @pattern
            OR registrants.family_name ILIKE @pattern
            OR churches.name ILIKE @pattern;";

        var pattern = $"%{searchParameters}%";
        var registrants = await _databaseService.QueryAsync<Registrant>(sql, new { pattern });
        if (registrants != null)
        {
            foreach (Registrant registrant in registrants)
            {
                return_value.Append(registrant);
            }
            return registrants;
        }

        return new List<Registrant> ();
    }

    private static Registrant MapRegistrantFromReader(DbDataReader reader)
    {
        // This should be able to return type Address Not CreateAddressRequest
        CreateAddressRequest registrantAddress = new CreateAddressRequest
        {
            StreetAddress1 = reader.GetString("StreetAddress1"),
            StreetAddress2 = reader.GetString("StreetAddress2"),
            City = reader.GetString("Locality"),
            State = reader.GetString("AdministrativeAreaLevel"),
            PostalCode = reader.GetString("PostalCode"),
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
                City = reader.GetString("ChurchLocality"),
                State = reader.GetString("ChurchAdministrativeAreaLevel"),
                PostalCode = reader.GetString("ChurchPostalCode"),
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