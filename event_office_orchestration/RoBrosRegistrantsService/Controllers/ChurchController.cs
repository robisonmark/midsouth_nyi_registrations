using System;
using System.Diagnostics;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using EventOfficeApi.Models;

namespace EventOfficeApi.Controllers
{
    [ApiController]
    public class ChurchController : BaseController
    {
        private readonly ILogger<ContactsController> _logger;

        public ContactsController(ILogger<ContactsController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        [Route("/api/church", Name = "AddChurch")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateChurch(Church church)
        {
            if (church.id == null)
            {
                church.id = Guid.NewGuid().ToString();
            }

            var sql = "INSERT INTO RoBrosRegistant.Church (Id, Name, Address) VALUES (@Name, @Email)";
            var rowsAffected = await ExecuteAsync(sql, church);

            if (rowsAffected > 0)
                return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);

            return BadRequest();
        }
    }
}