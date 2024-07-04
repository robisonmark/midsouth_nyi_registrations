using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EventOfficeApi.Controllers
{
    [ApiController]
    public class PersonController : ControllerBase
    {
        [HttpGet]
        [Route("api/person", Name = "GetPerson")]
        public async Task<IActionResult> GetPerson()
        {
            // TODO: Implement Database Access
            Person person = new Person
            {
                Id = 1,
                GivenName = "John",
                FamilyName = "Doe",
                CompetitionStatus = "Registered",
                Church = new Church
                {
                    Id = 1,
                    Name = "Church"
                },
                Address = new Address
                {
                    street_address_1 = "123 Main Street",
                    locality = "Anytown",
                    postal_code = 12345,
                    country = "USA",
                    administrative_area_level = "CA"
                }
            };

            return Ok(person);
        }
    }
}
