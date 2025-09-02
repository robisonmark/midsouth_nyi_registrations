using System;
using System.Diagnostics;
using System.Threading;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using EventOfficeApi.Models;
using EventOfficeApi.Services;

namespace EventOfficeApi.Controllers
{
    [ApiController]
    public class ChurchController : BaseController
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

            var sql = "INSERT INTO RoBrosRegistrant.Church (Id, Name, Address, CreatedAt) VALUES (@Id, @Name, @Address, NOW())";
            var parameters = new { Id = Guid.NewGuid(), Name = church.Name, Address = church.Address };
            // return await _databaseService.ExecuteAsync(sql, church);

            int rowsAffected = await _databaseService.ExecuteAsync(sql, parameters);

            if (rowsAffected > 0)
            {
                // return CreatedAtAction(nameof(GetChurch), new { id = church.Id }, church);
                return Accepted();
            }
            // await Task.Delay(1000);

            return BadRequest("Failed to add registrant.");
            // return Accepted();
        }
    }
}