using System;
using System.Diagnostics;
using System.Threading;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

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

        private readonly DatabaseService _databaseService;

        public ChurchController(DatabaseService databaseService)
        {
            _databaseService = databaseService;
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

            Guid addressId = Guid.NewGuid();
            church.createdBy = "Mark"; // replace with actual user
            church.updatedBy = "Mark"; // replace with actual user

            Console.WriteLine($"Creating church with ID: {church.Id}, Name: {church.Name}, Address: {addressId}");

            var sql = @"INSERT INTO church 
                        (Id, Name, CreatedBy, CreatedAt, UpdatedBy, UpdatedAt, Version) 
                        VALUES (@Id, @Name, 'Mark', NOW(), 'Mark', NOW(), 1)
                        RETURNING Id;";
            var parameters = new { Id = Guid.NewGuid(), Name = church.Name, AddressId = addressId };
            // return await _databaseService.ExecuteAsync(sql, church);

            int rowsAffected = await _databaseService.ExecuteAsync(sql, church);

            if (rowsAffected > 0)
            {
                // return CreatedAtAction(nameof(GetChurch), new { id = church.Id }, church);
                return Accepted(new { id = church.Id });
            }
            // await Task.Delay(1000);

            return BadRequest("Failed to add registrant.");
            // return Accepted();
        }

        [HttpGet]
        [ActionName(nameof(GetChurch))]
        [Route("/api/church/{churchId}", Name = "GetChurchById")]
        [ProducesResponseType(typeof(Church), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetChurch(Guid churchId)
        {
            var sql = "SELECT Id, Name, AddressId, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, Version FROM church WHERE Id = @Id";
            var parameters = new { Id = churchId };

            var response = await _databaseService.QuerySingleAsync<Church>(sql, parameters);

            if (response == null)
            {
                return NotFound();
            }
            return Ok(response);
        }
    }
}