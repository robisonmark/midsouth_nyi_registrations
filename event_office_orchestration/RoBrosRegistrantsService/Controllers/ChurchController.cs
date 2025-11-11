// Standard Libraries
using System;
using System.Diagnostics;
using System.Threading;

// ASP.NET Libraries
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

// RoBros Libraries
using EventOfficeApi.RoBrosAddressesService.Models;

// Local
using RoBrosRegistrantsService.Models;
using RoBrosRegistrantsService.Services;
// using RoBrosRegistrantsService.Data;

namespace RoBrosRegistrantsService.Controllers
{
    [ApiController]
    public class ChurchController : ControllerBase
    {
        // private readonly ILogger<ContactsController> _logger;

        // public ContactsController(ILogger<ContactsController> logger)
        // {
        //     _logger = logger;
        // }

        private readonly ChurchService _churchService;

        public ChurchController(ChurchService churchService)
        {
            _churchService = churchService;
        }

        [HttpPost]
        [Route("/api/church", Name = "AddChurch")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateChurch([FromBody] Church church)
        {
            // Guid church.Id = request.Id ?? Guid.NewGuid();  // If null, generate a new GUID

            if (church.Id == null)
            {
                church.Id = Guid.NewGuid();
            }

            if (church.Address == null)
            {
                // This would need to call the address service to create a new address and return the ID
                church.Address = new Address();
                // This should also create a look up table for addresses
            }

            church.createdBy = "Mark"; // replace with actual user
            church.updatedBy = "Mark"; // replace with actual user

            try
            {
                var id = await _churchService.CreateChurchAsync(church);
                if (id == Guid.Empty)
                {
                    return BadRequest("Failed to create church");
                }

                return Accepted(new { id });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return BadRequest("Failed to add church.");
            }
        }

        [HttpGet]
        [ActionName(nameof(GetChurch))]
        [Route("/api/church/{churchId}", Name = "GetChurchById")]
        [ProducesResponseType(typeof(Church), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetChurch(Guid churchId)
        {
            var response = await _churchService.GetChurchAsync(churchId);
            if (response == null)
            {
                return NotFound();
            }
            return Ok(response);
        }
    }
}