using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EvolutionMetrics.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class DataController : ControllerBase
    {
        private readonly ILogger<DataController> _logger;

        public DataController(ILogger<DataController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Returns the available filter options for brands, processors, and exercise types.
        /// </summary>
        [HttpGet("options")]
        public IActionResult GetOptions()
        {
            try
            {
                var laptopLines = System.IO.File.ReadAllLines("Dataset/laptop.csv")
                    .Skip(1)
                    .ToArray();

                var brands = laptopLines
                    .Select(x => x.Split(',')[0])
                    .Distinct();

                var processors = laptopLines
                    .Select(x => x.Split(',')[3])
                    .Distinct();

                var exercises = System.IO.File.ReadAllLines("Dataset/calories.csv")
                    .Skip(1)
                    .Select(x => x.Split(',')[3])
                    .Distinct();

                _logger.LogInformation("Dataset options fetched successfully");

                return Ok(new { brands, processors, exercises });
            }
            catch (IOException ex)
            {
                _logger.LogError(ex, "Failed to read dataset files");
                throw;
            }
        }
    }
}
