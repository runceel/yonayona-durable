using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;

namespace Functions;

public static class WeatherOrchestrator
{
    /// <summary>
    /// 都市一覧を取得し、天気取得アクティビティをバッチ実行して平均気温を算出するオーケストレーター関数
    /// </summary>
    [Function(nameof(WeatherOrchestrator))]
    public static async Task<WeatherResult> RunOrchestrator(
        [OrchestrationTrigger] TaskOrchestrationContext context)
    {
        // 最初に対象都市の一覧をアクティビティ関数から取得する
        var cities = await context.CallActivityAsync<string[]>(
            nameof(WeatherActivities.GetCities));

        // 取得した各都市の天気結果を蓄積する
        List<CityWeather> weatherList = [];

        // 一度に呼び出す件数を抑えるため、20 件ずつに分割して処理する
        foreach (var cityBatch in cities.Chunk(20))
        {
            // バッチ内の都市について天気取得アクティビティを並列実行する
            var tasks = cityBatch
                .Select(city => context.CallActivityAsync<CityWeather>(
                    nameof(WeatherActivities.FetchWeather), city));

            var batchResult = await Task.WhenAll(tasks);

            // バッチ結果を全体の結果リストに追加する
            weatherList.AddRange(batchResult);
        }

        // すべての都市の結果から平均気温を算出して返す
        var average = weatherList.Average(w => w.Temperature);
        return new WeatherResult([.. weatherList], average);
    }
}
