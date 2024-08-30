using System;
using System.Diagnostics;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using EventOfficeApi.Models;

namespace EventOfficeApi.Controllers
{
    [ApiController]
    public class PersonController : ControllerBase
    {
        // private readonly ILogger<ContactsController> _logger;

        // public ContactsController(ILogger<ContactsController> logger)
        // {
        //     _logger = logger;
        // }

        [HttpGet]
        [Route("/api/person", Name = "GetPersonById")]
        [ProducesResponseType(typeof(Registrant), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPerson(Guid personId)
        {
            // TODO: Implement Database Access
            Registrant person = new Registrant
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

            return Ok(person);
        }


        [HttpPost]
        [Route("/api/person", Name = "AddPerson")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreatePerson(Registrant person)
        {
            // TODO: Implement Database Access

            await Task.Delay(10);

            return Ok(person);
        }

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
}
