using System;
using System.Diagnostics;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using EventOfficeApi.Models;

namespace EventOfficeApi.Controllers
{
    [ApiController]
<<<<<<< HEAD
    public class RegistrantController : BaseController
=======
    public class RegistrantController : ControllerBase
>>>>>>> aaadf4e (Feature/addresses service (#8))
    {
        // private readonly ILogger<ContactsController> _logger;

        // public ContactsController(ILogger<ContactsController> logger)
        // {
        //     _logger = logger;
        // }

        [HttpPost]
        [Route("/api/registrant", Name = "AddRegistrant")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateRegistrant(Registrant registrant)
        {
<<<<<<< HEAD
            if (registrant.id == null)
            {
                registrant.id = Guid.NewGuid().ToString();
            }
            var sql = "INSERT INTO RoBrosRegistant.Registrant (Id, GivenName, FamilyName, Particpant) VALUES (@Name, @Email)";
            var rowsAffected = await ExecuteAsync(sql, registrant);

            if (rowsAffected > 0)
                return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);

            return BadRequest();
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
                    Id = 1,
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
=======
            // TODO: Implement Database Access
>>>>>>> aaadf4e (Feature/addresses service (#8))

            await Task.Delay(10);

            return Ok(registrant);
        }
<<<<<<< HEAD
=======

        private class Address : IAddress
        {
            required public string StreetAddress1 { get; set; }
            public string? StreetAddress2 { get; set; }
            required public string Locality { get; set; }
            public int PostalCode { get; set; }
            required public string Country { get; set; }
            required public string AdministrativeAreaLevel { get; set; }
        }
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
                Id = 1,
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
>>>>>>> aaadf4e (Feature/addresses service (#8))
    }

}
