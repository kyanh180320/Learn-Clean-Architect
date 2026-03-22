using Application.UseCases;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace Learn.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly GetWeatherForecastsUseCase _useCase;

        public WeatherForecastController(GetWeatherForecastsUseCase useCase)
        {
            _useCase = useCase;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public Task<IReadOnlyList<WeatherForecast>> Get([FromQuery] int count = 5, CancellationToken cancellationToken = default)
        {
            return _useCase.ExecuteAsync(count, cancellationToken);
        }
    }
}
