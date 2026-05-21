using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;

namespace Functions;

public static class WeatherOrchestrator
{
    [Function(nameof(WeatherOrchestrator))]
    public static async Task<WeatherResult> RunOrchestrator(
        [OrchestrationTrigger] TaskOrchestrationContext context)
    {
        var cities = await context.CallActivityAsync<string[]>(
            nameof(WeatherActivities.GetCities));

        var tasks = cities
            .Select(city => context.CallActivityAsync<CityWeather>(
                nameof(WeatherActivities.FetchWeather), city))
            .ToList();

        var weathers = await Task.WhenAll(tasks);

        var average = weathers.Average(w => w.Temperature);
        return new WeatherResult(weathers, average);
    }
}
