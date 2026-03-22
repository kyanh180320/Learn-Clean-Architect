using Domain.Models;

namespace Application.Ports
{
    public interface IWeatherForecastRepository
    {
        Task<IReadOnlyList<WeatherForecast>> GetForecastsAsync(int count, CancellationToken cancellationToken);
    }
}

