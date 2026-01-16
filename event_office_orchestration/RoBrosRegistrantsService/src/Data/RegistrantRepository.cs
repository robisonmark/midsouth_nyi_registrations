using Microsoft.Extensions.Logging;
using Microsoft.Data.SqlClient;
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
    Task<IEnumerable<Competitor>> SearchRegistrantsAsync(string queryStringParameters);
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
                    INSERT INTO students (
                        registrant_id,
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
                        art_events,
                        creative_ministries_events,
                        creative_writing_events,
                        speech_events,
                        vocal_music_events,
                        instrumental_music_events,
                        individual_sports_events,
                        team_sports_events,
                        attending_tnt_at_tnu
                    ) VALUES (
                        @RegistrantId,
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
                        @ArtEvents,
                        @CreativeMinistriesEvents,
                        @CreativeWritingEvents,
                        @SpeechEvents,
                        @VocalMusicEvents,
                        @InstrumentalMusicEvents,
                        @IndividualSportEvents,
                        @TeamSportEvents,
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
                registrants.id, 
                given_name,
                family_name, 
                participant_role, 
                church_id, 
                youth_leader_email, 
                youth_leader_first_name, 
                youth_leader_last_name, 
                addresses.street_address_1, 
                cell_number, 
                email, 
                birthday,
                gender,
                shirt_size,
                art_events,
                creative_ministries_events,
                creative_writing_events,
                speech_events,
                vocal_music_events,
                instrumental_music_events,
                individual_sports_events,
                team_sports_events
            FROM registrants
                INNER JOIN churches ON registrants.church_id = churches.id
                INNER JOIN addresses ON registrants.address_id = addresses.id
                LEFT JOIN students ON registrants.id = students.registrant_id
            WHERE registrants.id = @id";

        var row = await _databaseService.QuerySingleAsync<dynamic>(sql, new { id });
        if (row == null)
        {
            throw new InvalidOperationException("Failed to get registrant.");
        }

        return MapRegistrantFromDatabaseRow(row);
        // var registrant = await _databaseService.QuerySingleAsync<Competitor>(sql, new { id });
        // if (registrant != null)
        // {
        //     return registrant;
        // }

        // throw new InvalidOperationException("Failed to get registrant.");
    }

    public async Task<List<Competitor>> GetAllRegistrantsAsync()
    {
        var sql = "SELECT * FROM registrant";
        
        // using var reader = await _databaseService.QueryAsync(sql);
        // if (await reader.ReadAsync())
        // {
        //     return MapRegistrantFromReader(reader);
        // }

        var registrants = (await _databaseService.QueryAsync<Competitor>(sql)).ToList();
        if (registrants != null)
        {
            return registrants;
        }

        throw new InvalidOperationException("Failed to registrants");
    }

    public async Task<Registrant> UpdateRegistrantAsync(Registrant registrant)
    {
         var sql = @"
            UPDATE registrant SET
                GivenName = @GivenName,
                FamilyName = @FamilyName, 
                ParticipantRole = @ParticipantRole, 
                ChurchId = @ChurchId, 
                YouthLeaderEmail = @YouthLeaderEmail, 
                YouthLeaderFirstName = @YouthLeaderFirstName, 
                YouthLeaderLastName = @YouthLeaderLastName, 
                AddressId = @AddressId, 
                Mobile = @Mobile, 
                Email = @Email, 
                Birthday = @Birthday,
                Gender = @Gender, 
                ShirtSize = @ShirtSize, 
                Price = @Price, 
                Paid = @Paid, 
                Notes = @Notes, 
                SubmissionDate = @SubmissionDate, 
                IPAddress = @IPAddress,
                UpdatedAt = @UpdatedAt,
                UpdatedBy = @UpdatedBy
            OUTPUT INSERTED.*
            WHERE Id = @Id";

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

    public async Task<IEnumerable<Competitor>> SearchRegistrantsAsync(string searchParameters)
    {
        IEnumerable<Competitor> return_value = new List<Competitor>();
        // Use a parameterized pattern for LIKE. Passing the % wildcard in the SQL string
        // around the parameter (e.g. '%@p%') does NOT substitute the parameter value.
        // Two safe options are: (1) build the pattern in .NET and pass it as a parameter,
        // or (2) use SQL concatenation: column LIKE '%' + @p + '%'.
         var sql = @"SELECT
            registrants.id AS Id,
            registrants.given_name AS GivenName,
            registrants.family_name AS FamilyName,
            registrants.participant_role AS ParticipantRole,
            registrants.church_id AS ChurchId,
            churches.name AS ChurchName,
            addresses.street_address_1 AS ChurchStreetAddress1,
            addresses.street_address_2 AS ChurchStreetAddress2,
            addresses.city AS ChurchLocality,
            addresses.state AS ChurchAdministrativeAreaLevel,
            addresses.postal_code AS ChurchPostalCode,
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
            registrants.ip_address AS IPAddress,
            students.art_events AS ArtEvents,
            students.creative_ministries_events AS CreativeMinistriesEvents,
            students.creative_writing_events AS CreativeWritingEvents,
            students.speech_events AS SpeechEvents,
            students.vocal_music_events AS VocalMusicEvents,
            students.instrumental_music_events AS InstrumentalMusicEvents,
            students.individual_sports_events AS IndividualSportsEvents,
            students.team_sports_events AS TeamSportsEvents
        FROM registrants
        INNER JOIN churches ON registrants.church_id = churches.id
        INNER JOIN addresses ON registrants.address_id = addresses.id
        LEFT JOIN students ON registrants.id = students.registrant_id
        WHERE registrants.given_name LIKE @pattern
            OR registrants.family_name LIKE @pattern
            OR churches.name LIKE @pattern;";

        var pattern = $"%{searchParameters}%";
        var rows = await _databaseService.QueryAsync<dynamic>(sql, new { pattern });
        if (rows == null)
        {
            return new List<Competitor>();
        }
        
        foreach (dynamic row in rows)
        {
            Competitor new_competitor = MapRegistrantFromDatabaseRow(row);
            return_value.Append(new_competitor);
        }
        return return_value;

    }

    private static Registrant MapRegistrantFromDatabaseRow(dynamic row)
    {
        // This should be able to return type Address Not CreateAddressRequest
        CreateAddressRequest registrantAddress = new CreateAddressRequest
        {
            StreetAddress1 = row.GetString("StreetAddress1"),
            StreetAddress2 = row.GetString("StreetAddress2"),
            City = row.GetString("Locality"),
            State = row.GetString("AdministrativeAreaLevel"),
            PostalCode = row.GetString("PostalCode"),
            Country = row.GetString("Country")
        }; 

        Church registrantChurch = new Church
        {
            Id = row.GetGuid("ChurchId"),
            Name = row.GetString("ChurchName"),
            Address = new CreateAddressRequest
            {
                StreetAddress1 = row.GetString("ChurchStreetAddress1"),
                StreetAddress2 = row.GetString("ChurchStreetAddress2"),
                City = row.GetString("ChurchLocality"),
                State = row.GetString("ChurchAdministrativeAreaLevel"),
                PostalCode = row.GetString("ChurchPostalCode"),
                Country = row.GetString("ChurchCountry")
            }
        };

        return new Registrant
        {
            Id = row.GetGuid("id"),
            GivenName = row.GetString("GivenName"),
            FamilyName = row.GetString("FamilyName"), 
            ParticipantRole = row.GetString("ParticipantRole"), 
            YouthLeaderEmail = row.GetString("YouthLeaderEmail"), 
            YouthLeaderFirstName = row.GetString("YouthLeaderFirstName"), 
            YouthLeaderLastName = row.GetString("YouthLeaderLastName"), 
            Mobile = row.GetString("Mobile"), 
            Email = row.GetString("Email"), 
            Birthday = row.GetDateTime("Birthday"),
            Gender = row.GetString("Gender"),
            ShirtSize = row.GetString("ShirtSize"),
            // These should return as whole but be stored as ID
            Address = registrantAddress, 
            Church = registrantChurch, 
            // Remove from Response
            Paid = row.GetBoolean("Paid"),
            SubmissionDate = row.GetDateTime("SubmissionDate"),
            IPAddress = row.GetString("IPAddress")
        };
    }
}