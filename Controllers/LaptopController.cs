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
    public class LaptopController : ControllerBase
    {
        private readonly LaptopService _service;

        public LaptopController(LaptopService service)
        {
            _service = service;
        }

        [HttpPost("predict")]
        public IActionResult Predict([FromBody] LaptopInput input)
        {

            Log.Information("📥 API Hit: Laptop");

            var data = new LaptopData
            {
                Brand = input.Brand,
                RAM = input.RAM,
                Storage = input.Storage,
                Processor = input.Processor,
                ScreenSize = input.ScreenSize
            };

            var result = _service.Predict(data);

            Log.Information("📤 API Response Laptop = {Result}", result);

            return Ok(new
            {
                predictedPrice = result,
                mae = _service.Metrics.MAE,
                rmse = _service.Metrics.RMSE,
                r2 = _service.Metrics.R2
            });
        }

    }
}
