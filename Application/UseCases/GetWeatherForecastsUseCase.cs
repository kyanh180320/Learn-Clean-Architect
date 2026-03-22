using Application.Ports;
using Domain.Models;

namespace Application.UseCases
{
    public class GetWeatherForecastsUseCase
    {
        private readonly IWeatherForecastRepository _weatherForecastRepository;

        public GetWeatherForecastsUseCase(IWeatherForecastRepository weatherForecastRepository)
        {
            _weatherForecastRepository = weatherForecastRepository;
        }

        public Task<IReadOnlyList<WeatherForecast>> ExecuteAsync(int count, CancellationToken cancellationToken)
        {
            return _weatherForecastRepository.GetForecastsAsync(count, cancellationToken);
        }
    }
}

