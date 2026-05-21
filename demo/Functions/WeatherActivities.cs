using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Functions;

public static class WeatherActivities
{
    private static readonly string[] DefaultCities =
        ["東京", "大阪", "札幌", "福岡", "那覇", "仙台"];

    [Function(nameof(GetCities))]
    public static string[] GetCities([ActivityTrigger] object? _) => DefaultCities;

    [Function(nameof(FetchWeather))]
    public static async Task<CityWeather> FetchWeather(
        [ActivityTrigger] string city,
        FunctionContext context)
    {
        var logger = context.GetLogger(nameof(FetchWeather));
        logger.LogInformation("Fetching weather for {City}", city);

        // 進捗を見せるためのダミー遅延（5〜10 秒のランダム）
        var delaySeconds = Random.Shared.Next(5, 11);
        await Task.Delay(TimeSpan.FromSeconds(delaySeconds));

        var temperature = Random.Shared.Next(-5, 35);
        logger.LogInformation("{City}: {Temp}°C (took {Delay}s)", city, temperature, delaySeconds);

        return new CityWeather(city, temperature);
    }
}
