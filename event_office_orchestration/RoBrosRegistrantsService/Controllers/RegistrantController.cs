// Standard Libraries
using System;
using System.Diagnostics;

// ASP.NET Libraries
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

// RoBros Libraries
using EventOfficeApi.RoBrosAddressesService.Models;

// Local
using RoBrosRegistrantsService.Models;
using RoBrosRegistrantsService.Services;

// TODO: Move this implementation to a different file

namespace RoBrosRegistrantsService.Controllers
{
    [ApiController]
    public class RegistrantController : ControllerBase
    {

        private readonly IRegistrantService _registrantService;

        public RegistrantController(IRegistrantService registrantService)
        {
            _registrantService = registrantService;
        }


        [HttpPost]
        [Route("/api/registrant", Name = "AddRegistrant")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<Guid> CreateRegistrantAsync(Registrant registrant)
        {
            registrant.Id = Guid.NewGuid();
           
           return await _registrantService.CreateRegistrantAsync(registrant);
        }

        [HttpGet]
        // [ActionName(nameof(GetRegistrant))]
        [Route("/api/registrant/{registrantId}", Name = "GetRegistrantById")]
        [ProducesResponseType(typeof(Registrant), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetRegistrantAsync(Guid registrantId)
        {
            // TODO: Implement Database Access
            return await _registrantService.GetRegistrantAsync(registrantId) is Registrant registrant
                ? Ok(registrant)
                : NotFound();
        }

        [HttpGet]
        // [ActionName(nameof(SearchRegistrants))]
        [Route("/api/registrants/search/{searchParameters}", Name = "GetRegistrantsSearch")]
        [ProducesResponseType(typeof(IEnumerable<Registrant>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> SearchRegistrantsAsync(string searchParameters)
        {
            return await _registrantService.SearchRegistrantsAsync(searchParameters) is IEnumerable<Registrant> registrants && registrants.Any()
                ? Ok(registrants)
                : NoContent();
        }
    }
}
