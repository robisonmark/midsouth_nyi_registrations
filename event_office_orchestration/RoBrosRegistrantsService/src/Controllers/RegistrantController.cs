// Standard Libraries
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

// ASP.NET Libraries
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

// Newtonsoft for flexible JSON deserialization
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

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
        public async Task<IActionResult> CreateRegistrantAsync([FromBody] JObject body)
        {
            if (body == null)
            {
                return BadRequest("Request body is required.");
            }

            // Determine which concrete type the caller sent. Prefer Competitor when
            // competition status indicates competing or when competitor-specific
            // props are present.
            Registrant registrant;

            try
            {
                var competitionStatus = body["competitionStatus"]?.ToString();
                var participantRole = body["ParticipantRole"]?.ToString();

                Console.WriteLine($"\n \n CompetitionStatus: {competitionStatus}");

                var looksLikeCompetitor = false;
                if (!string.IsNullOrWhiteSpace(competitionStatus) && competitionStatus.Equals("competing", StringComparison.OrdinalIgnoreCase))
                {
                    looksLikeCompetitor = true;
                }

                // Also treat presence of competitor-specific fields as indicator
                if (!looksLikeCompetitor)
                {
                    var competitorFields = new[] { "District", "Quizzing", "AttendingTNTatTNU" };
                    if (competitorFields.Any(f => body[f] != null))
                    {
                        looksLikeCompetitor = true;
                    }
                }

                if (looksLikeCompetitor)
                {
                    var comp = body.ToObject<RoBrosRegistrantsService.Models.Competitor>();
                    if (comp == null) throw new JsonException("Unable to deserialize body as Competitor");
                    registrant = comp;
                }
                else if (!string.IsNullOrWhiteSpace(participantRole) && participantRole.Equals("Student", StringComparison.OrdinalIgnoreCase))
                {
                    var student = body.ToObject<RoBrosRegistrantsService.Models.Student>();
                    if (student == null) throw new JsonException("Unable to deserialize body as Student");
                    registrant = student;
                }
                else
                {
                    var reg = body.ToObject<RoBrosRegistrantsService.Models.Registrant>();
                    if (reg == null) throw new JsonException("Unable to deserialize body as Registrant");
                    registrant = reg;
                }
            }
            catch (JsonException je)
            {
                return BadRequest($"Invalid JSON body: {je.Message}");
            }

            if (registrant == null)
            {
                return BadRequest("Unable to parse registrant payload.");
            }

            registrant.Id = Guid.NewGuid();

            var id = await _registrantService.CreateRegistrantAsync(registrant);

            // Return 201 with location header pointing to the GET endpoint
            return CreatedAtRoute("GetRegistrantById", new { registrantId = id }, id);
        }

        [HttpGet]
        // [ActionName(nameof(GetRegistrant))]
        [Route("/api/registrant/{registrantId}", Name = "GetRegistrantById")]
        [ProducesResponseType(typeof(Registrant), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetRegistrantAsync(Guid registrantId)
        {
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
