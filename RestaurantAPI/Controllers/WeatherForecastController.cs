using Microsoft.AspNetCore.Mvc;
using static System.Runtime.InteropServices.JavaScript.JSType;
namespace RestaurantAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly IWeatherForcastService _service;
        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IWeatherForcastService service)
        {
            _logger = logger;
            _service = service;
        }

        [HttpPost("generate")]
        public ActionResult<IEnumerable<WeatherForecast>> Generate([FromQuery] int count,
            [FromBody]TemperatureRequest request)
        {
            if (count < 1 || request.Min > request.Max) return BadRequest();
            var result = _service.Get(count, request.Min, request.Max);
            return Ok(result);
        }
    }
}
