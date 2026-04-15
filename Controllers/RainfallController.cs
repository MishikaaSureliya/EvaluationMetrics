using EvolutionMetrics.Models;
using EvolutionMetrics.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EvolutionMetrics.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class RainfallController : ControllerBase
    {
        private const int MAX_RECORD_LIMIT = 250;

        private readonly IRainfallService _rainfallService;
        private readonly ILogger<RainfallController> _logger;

        public RainfallController(IRainfallService rainfallService, ILogger<RainfallController> logger)
        {
            _rainfallService = rainfallService;
            _logger = logger;
        }

        /// <summary>
        /// Predicts rainfall for each record in the provided weather dataset.
        /// </summary>
        [HttpPost("predict")]
        public IActionResult Predict([FromBody] RainfallRequest request)
        {
            _logger.LogInformation("API Hit: Rainfall");

            if (request.Data == null || request.Data.Count == 0)
            {
                return BadRequest("No data received");
            }

            if (request.Data.Count > MAX_RECORD_LIMIT)
            {
                _logger.LogWarning("Rainfall limit exceeded: {Count}", request.Data.Count);

                return BadRequest(new
                {
                    status = "failed",
                    message = $"Max {MAX_RECORD_LIMIT} records allowed",
                    received = request.Data.Count
                });
            }

            var predictions = new List<object>();

            foreach (var input in request.Data)
            {
                var result = _rainfallService.Predict(
                    input.Temperature,
                    input.Humidity,
                    input.WindSpeed,
                    input.Pressure
                );

                predictions.Add(new
                {
                    prediction = result.prediction,
                    isDummy = result.isDummy
                });
            }

            _logger.LogInformation("Rainfall predictions completed for {Count} records", request.Data.Count);

            return Ok(new
            {
                status = "success",
                count = request.Data.Count,
                predictions = predictions,
                mae = double.IsNaN(_rainfallService.Metrics.MAE) || double.IsInfinity(_rainfallService.Metrics.MAE) ? 0 : _rainfallService.Metrics.MAE,
                rmse = double.IsNaN(_rainfallService.Metrics.RMSE) || double.IsInfinity(_rainfallService.Metrics.RMSE) ? 0 : _rainfallService.Metrics.RMSE,
                r2 = double.IsNaN(_rainfallService.Metrics.R2) || double.IsInfinity(_rainfallService.Metrics.R2) ? 0 : _rainfallService.Metrics.R2
            });
        }
    }
}