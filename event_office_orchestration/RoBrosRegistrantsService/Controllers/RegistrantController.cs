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

        // public ChurchRepository(DatabaseService databaseService)
        // {
        //     _databaseService = databaseService;
        // }

        [HttpPost]
        [Route("/api/registrant", Name = "AddRegistrant")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateRegistrant(Registrant registrant)
        {
            if (registrant.Id == null)
            {
                registrant.Id = Guid.NewGuid();
            }
            var sql = "INSERT INTO RoBrosRegistant.Registrant (Id, GivenName, FamilyName, Participant) VALUES (@Name, @Email)";
            // return await _databaseService.ExecuteAsync(sql, registrant);
            int rowsAffected = await _databaseService.ExecuteAsync(sql, registrant);

            if (rowsAffected > 0)
            {
                return CreatedAtAction(nameof(GetRegistrant), new { id = registrant.Id }, registrant);
            }

            return BadRequest("Failed to add registrant.");
        }

        [HttpGet]
        [Route("/api/registrant", Name = "GetRegistrantById")]
        [ProducesResponseType(typeof(Registrant), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetRegistrant(Guid registrantId)
        {
            // TODO: Implement Database Access
            Registrant registrant = new Registrant
            {
                Id = Guid.NewGuid(),
                GivenName = "John",
                FamilyName = "Doe",
                ParticpantRole = "Competitor",
                Church = new Church
                {
                    Id = Guid.NewGuid(),
                    Name = "Church"
                },
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
