using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Functions;

public static class WeatherActivities
{
    private static readonly string[] DefaultCities =
    [
        "札幌", "函館", "青森", "盛岡", "仙台", "秋田", "山形", "福島", "水戸", "宇都宮",
        "前橋", "さいたま", "千葉", "東京", "横浜", "新潟", "富山", "金沢", "福井", "甲府",
        "長野", "岐阜", "静岡", "名古屋", "津", "大津", "京都", "大阪", "神戸", "奈良",
        "和歌山", "鳥取", "松江", "岡山", "広島", "山口", "徳島", "高松", "松山", "高知",
        "福岡", "佐賀", "長崎", "熊本", "大分", "宮崎", "鹿児島", "那覇", "旭川", "北九州"
    ];

    /// <summary>
    /// 都市名のリストを返すアクティビティ関数
    /// </summary>
    [Function(nameof(GetCities))]
    public static string[] GetCities([ActivityTrigger] object? _) => DefaultCities;

    /// <summary>
    /// 都市名を受け取り、天気情報を返すアクティビティ関数
    /// </summary>
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
