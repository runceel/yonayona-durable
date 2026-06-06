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

        List<CityWeather> weatherList = [];

        foreach (var cityBatch in cities.Chunk(10))
        {
            var tasks = cityBatch
                .Select(city => context.CallActivityAsync<CityWeather>(
                    nameof(WeatherActivities.FetchWeather), city));

            var batchResult = await Task.WhenAll(tasks);
            weatherList.AddRange(batchResult);
        }

        var average = weatherList.Average(w => w.Temperature);
        return new WeatherResult([.. weatherList], average);
    }
}
