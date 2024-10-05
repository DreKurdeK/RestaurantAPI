namespace RestaurantAPI
{
    public interface IWeatherForcastService
    {
        IEnumerable<WeatherForecast> Get(int count, int minTemp, int maxTemp);
    }
}