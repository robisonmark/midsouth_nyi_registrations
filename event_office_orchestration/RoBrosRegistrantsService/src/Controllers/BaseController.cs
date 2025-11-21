using Microsoft.AspNetCore.Mvc;

namespace RoBrosRegistrantsService.Controllers
{
    [ApiController]
    public abstract class BaseController : ControllerBase
    {
        protected IActionResult HandleResult<T>(T result)
        {
            if (result == null)
                return NotFound();

            return Ok(result);
        }

        protected IActionResult HandleException(Exception ex)
        {
            return StatusCode(500, new { Message = "An error occurred", Details = ex.Message });
        }
    }
}
