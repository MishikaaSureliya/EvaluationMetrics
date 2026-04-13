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
    public class RainfallController : ControllerBase
    {
        private readonly RainfallService _service;

        public RainfallController(RainfallService service)
        {
            _service = service;
        }

        [HttpPost("predict")]
        public IActionResult Predict([FromBody] RainfallInput input)
        {
            Log.Information("📥 API Hit: Rainfall Predict");
            var result = _service.Predict(
                input.Temperature,
                input.Humidity,
                input.WindSpeed,
                input.Pressure
            );
            Log.Information("📥 API Hit: Rainfall Predict");

            return Ok(new
            {
                predictedRainfall =  result.prediction,
                isDummy = result.isDummy,   // 🔥 IMPORTANT

                mae = double.IsNaN(_service.Metrics.MAE) || double.IsInfinity(_service.Metrics.MAE) ? 0 : _service.Metrics.MAE,
                rmse = double.IsNaN(_service.Metrics.RMSE) || double.IsInfinity(_service.Metrics.RMSE) ? 0 : _service.Metrics.RMSE,
                r2 = double.IsNaN(_service.Metrics.R2) || double.IsInfinity(_service.Metrics.R2) ? 0 : _service.Metrics.R2
            });
        }
    }
}