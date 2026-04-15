using EvolutionMetrics.Models;
using EvolutionMetrics.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EvolutionMetrics.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CaloriesController : ControllerBase
    {
        private const int MAX_RECORD_LIMIT = 250;

        private readonly ICaloriesService _caloriesService;
        private readonly ILogger<CaloriesController> _logger;

        public CaloriesController(ICaloriesService caloriesService, ILogger<CaloriesController> logger)
        {
            _caloriesService = caloriesService;
            _logger = logger;
        }

        /// <summary>
        /// Predicts calories burned for each record in the provided dataset.
        /// </summary>
        [HttpPost("predict")]
        public IActionResult Predict([FromBody] CaloriesRequest request)
        {
            _logger.LogInformation("API Hit: Calories");

            if (request.Data == null || request.Data.Count == 0)
            {
                return BadRequest("No data received");
            }

            if (request.Data.Count > MAX_RECORD_LIMIT)
            {
                _logger.LogWarning("Calories limit exceeded. Received {Count}", request.Data.Count);

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
                var data = new CaloriesData
                {
                    Weight = input.Weight,
                    Duration = input.Duration,
                    HeartRate = input.HeartRate,
                    ExerciseType = input.ExerciseType
                };

                var prediction = _caloriesService.Predict(data);
                results.Add(prediction);
            }

            _logger.LogInformation("Calories API success for {Count} records", request.Data.Count);

            return Ok(new
            {
                status = "success",
                count = request.Data.Count,
                predictions = results,
                mae = _caloriesService.Metrics.MAE,
                rmse = _caloriesService.Metrics.RMSE,
                r2 = _caloriesService.Metrics.R2
            });
        }
    }
}
