using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EvolutionMetrics.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class DataController : ControllerBase
    {
        [HttpGet("options")]
        public IActionResult GetOptions()
        {
            var brands = System.IO.File.ReadAllLines("Dataset/laptop.csv")
                .Skip(1)
                .Select(x => x.Split(',')[0])
                .Distinct();

            var processors = System.IO.File.ReadAllLines("Dataset/laptop.csv")
                .Skip(1)
                .Select(x => x.Split(',')[3])
                .Distinct();

            var exercises = System.IO.File.ReadAllLines("Dataset/calories.csv")
                .Skip(1)
                .Select(x => x.Split(',')[3])
                .Distinct();

            return Ok(new { brands, processors, exercises });
        }
    }
}
