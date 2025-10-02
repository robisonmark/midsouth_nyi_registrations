using System;
using System.Diagnostics;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using EventOfficeApi.Models;
using EventOfficeApi.Services;

namespace EventOfficeApi.Controllers
{
    [ApiController]
    public class RegistrantController : ControllerBase
    {
        // private readonly ILogger<ContactsController> _logger;

        // public ContactsController(ILogger<ContactsController> logger)
        // {
        //     _logger = logger;
        // }

        private readonly DatabaseService _databaseService;

        public RegistrantController(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        [HttpPost]
        [Route("/api/registrant", Name = "AddRegistrant")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateRegistrant(Registrant registrant)
        {
            // if (registrant.Id == null)
            // {
            //     registrant.Id = Guid.NewGuid();
            // }

            registrant.Id = Guid.NewGuid();

            if (registrant.Address == null)
            {
                registrant.Address = new Address();
            }
            Console.WriteLine($"Creating registrant with ID: {registrant.Address}");

            var sql = "INSERT INTO registrant (Id, GivenName, FamilyName, ParticipantRole, ChurchId, YouthLeaderEmail, YouthLeaderFirstName, YouthLeaderLastName, AddressId, Mobile, Email, Birthday, Gender, ShirtSize, Price, Paid, Notes, SubmissionDate, IPAddress) VALUES (@Id, @GivenName, @FamilyName, @ParticipantRole, @Church, @YouthLeaderEmail, @YouthLeaderFirstName, @YouthLeaderLastName, @Address, @Mobile, @Email, @Birthday, @Gender, @ShirtSize, @Price, @Paid, @Notes, @SubmissionDate, @IPAddress)";
            // return await _databaseService.ExecuteAsync(sql, registrant);

            int rowsAffected = await _databaseService.ExecuteAsync(sql, registrant);
            Console.WriteLine($"Rows affected: {rowsAffected}");
            if (rowsAffected > 0)
            {
                return CreatedAtAction(nameof(GetRegistrant), new { registrantId = registrant.Id }, registrant);
            }

            return BadRequest("Failed to add registrant.");
        }

        [HttpGet]
        [ActionName(nameof(GetRegistrant))]
        [Route("/api/registrant/{registrantId}", Name = "GetRegistrantById")]
        [ProducesResponseType(typeof(Registrant), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetRegistrant(Guid registrantId)
        {
            // TODO: Implement Database Access
            Registrant registrant = new Registrant
            {
                Id = registrantId,
                GivenName = "John",
                FamilyName = "Doe",
                ParticipantRole = "Competitor",
                // Church = new Church
                // {
                //     Id = Guid.NewGuid(),
                //     Name = "Church"
                // },
                Church = "Hendersonville Church of the Nazarene",
                Address = new Address
                {
                    StreetAddress1 = "123 Main Street",
                    Locality = "Anytown",
                    PostalCode = 12345,
                    Country = "USA",
                    AdministrativeAreaLevel = "CA"
                },
                SubmissionDate = DateTime.UtcNow,
                IPAddress = "",
                Paid = false,
            };

            await Task.Delay(10);

            return Ok(registrant);
        }
    }
}
