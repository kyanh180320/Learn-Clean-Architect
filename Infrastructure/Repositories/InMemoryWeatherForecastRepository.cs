using Application.Ports;
using Domain.Models;

namespace Infrastructure.Repositories
{
    public class InMemoryWeatherForecastRepository : IWeatherForecastRepository
    {
        private static readonly string[] Summaries =
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering",
            "Scorching"
        };

        public Task<IReadOnlyList<WeatherForecast>> GetForecastsAsync(int count, CancellationToken cancellationToken)
        {
            if (count <= 0)
            {
                return Task.FromResult<IReadOnlyList<WeatherForecast>>(Array.Empty<WeatherForecast>());
            }

            cancellationToken.ThrowIfCancellationRequested();

            var forecasts = Enumerable.Range(1, count).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            }).ToArray();

            return Task.FromResult<IReadOnlyList<WeatherForecast>>(forecasts);
        }
    }
}

