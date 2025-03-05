using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/calculateage")]
    public class AgeCalculatorController : ControllerBase
    {
        [HttpGet]
        public IActionResult Greet([FromQuery] string name = "anonymous", 
            [FromQuery] int year = 0, 
            [FromQuery] int month = 0, 
            [FromQuery] int day = 0)
        {
            if (year == 0 || month == 0 || day == 0)
                return BadRequest("I can’t calculate your age without knowing your birthdate!");

            DateTime birthDate;
            try
            {
                birthDate = new DateTime(year, month, day);
            }
            catch
            {
                return BadRequest("Invalid date provided.");
            }

            DateTime today = DateTime.Today;
            int age = today.Year - birthDate.Year;

            // if the birthday hasn't occurred yet this year
            if (today < birthDate.AddYears(age))
            {
                age--;
            }

            return Ok($"Hello, {name}. You are {age} years old.");
        }
    }
}