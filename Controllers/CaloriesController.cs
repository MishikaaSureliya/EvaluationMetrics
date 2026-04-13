using EvolutionMetrics.Models;
using EvolutionMetrics.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace EvolutionMetrics.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CaloriesController : ControllerBase
    {
        private readonly CaloriesService _service;
        public CaloriesController(CaloriesService service)
        {
            _service = service;
        }

        [HttpPost("predict")]
        public IActionResult Predict([FromBody] CaloriesInput input)
        {
            Log.Information("📥 API Hit: Calories");

            var data = new CaloriesData
            {
                Weight = input.Weight,
                Duration = input.Duration,
                HeartRate = input.HeartRate,
                ExerciseType = input.ExerciseType
            };

            var result = _service.Predict(data);

            Log.Information("📤 API Response Calories = {Result}", result);

            return Ok(new
            {
                predictedCalories = result,
                mae = _service.Metrics.MAE,
                rmse = _service.Metrics.RMSE,
                r2 = _service.Metrics.R2
            });
        }
    }
}
