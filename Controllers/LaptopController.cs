using EvolutionMetrics.Models;
using EvolutionMetrics.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EvolutionMetrics.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class LaptopController : ControllerBase
    {
        private const int MAX_RECORD_LIMIT = 250;

        private readonly ILaptopService _laptopService;
        private readonly ILogger<LaptopController> _logger;

        public LaptopController(ILaptopService laptopService, ILogger<LaptopController> logger)
        {
            _laptopService = laptopService;
            _logger = logger;
        }

        /// <summary>
        /// Predicts the price for each laptop record in the provided dataset.
        /// </summary>
        [HttpPost("predict")]
        public IActionResult Predict([FromBody] LaptopRequest request)
        {
            _logger.LogInformation("API Hit: Laptop");

            if (request.Data == null || request.Data.Count == 0)
            {
                return BadRequest("No data received");
            }

            if (request.Data.Count > MAX_RECORD_LIMIT)
            {
                _logger.LogWarning("Laptop limit exceeded: {Count}", request.Data.Count);

                return BadRequest(new
                {
                    status = "failed",
                    message = $"Max {MAX_RECORD_LIMIT} records allowed",
                    received = request.Data.Count
                });
            }

            var results = new List<float>();

            foreach (var input in request.Data)
            {
                var data = new LaptopData
                {
                    Brand = input.Brand,
                    RAM = input.RAM,
                    Storage = input.Storage,
                    Processor = input.Processor,
                    ScreenSize = input.ScreenSize
                };

                var prediction = _laptopService.Predict(data);
                results.Add(prediction);
            }

            _logger.LogInformation("Laptop predictions completed for {Count} records", request.Data.Count);

            return Ok(new
            {
                status = "success",
                count = request.Data.Count,
                predictions = results,
                mae = _laptopService.Metrics.MAE,
                rmse = _laptopService.Metrics.RMSE,
                r2 = _laptopService.Metrics.R2
            });
        }
    }
}
