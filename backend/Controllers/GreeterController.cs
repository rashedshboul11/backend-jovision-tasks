using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GreeterController : ControllerBase
    {
        [HttpGet]
        public IActionResult Greet([FromQuery] string name = "anonymous")
        {
            return Ok($"Hello {name}");
            
        }
    }
}