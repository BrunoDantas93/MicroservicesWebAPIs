using LogProcessorAPI.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace LogProcessorAPI.Controllers;
[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;
    private readonly LogProcessor _logs;

    public WeatherForecastController(ILogger<WeatherForecastController> logger, LogProcessor logs)
    {
        _logger = logger;
        _logs = logs;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public async Task<ActionResult> Get()
    {
        await _logs.ProcessarLogs();

        return Ok();
    }
}
