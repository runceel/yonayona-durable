using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Functions;

public static class WeatherActivities
{
    // オーケストレーターが処理対象とする都道府県一覧
    private static readonly string[] DefaultCities =
    [
        "北海道", "青森県", "岩手県", "宮城県", "秋田県", "山形県", "福島県",
        "茨城県", "栃木県", "群馬県", "埼玉県", "千葉県", "東京都", "神奈川県",
        "新潟県", "富山県", "石川県", "福井県", "山梨県", "長野県",
        "岐阜県", "静岡県", "愛知県", "三重県",
        "滋賀県", "京都府", "大阪府", "兵庫県", "奈良県", "和歌山県",
        "鳥取県", "島根県", "岡山県", "広島県", "山口県",
        "徳島県", "香川県", "愛媛県", "高知県",
        "福岡県", "佐賀県", "長崎県", "熊本県", "大分県", "宮崎県", "鹿児島県", "沖縄県"
    ];

    /// <summary>
    /// オーケストレーターから呼び出され、処理対象となる都市名の一覧を返すアクティビティ関数
    /// </summary>
    [Function(nameof(GetCities))]
    public static string[] GetCities([ActivityTrigger] object? _) => DefaultCities;

    /// <summary>
    /// 1 都市分の天気情報を取得して返すアクティビティ関数
    /// </summary>
    [Function(nameof(FetchWeather))]
    public static async Task<CityWeather> FetchWeather(
        [ActivityTrigger] string city,
        FunctionContext context)
    {
        var logger = context.GetLogger(nameof(FetchWeather));
        logger.LogInformation("Fetching weather for {City}", city);

        // 外部 API 呼び出し相当の待機時間を模したダミー遅延（5〜10 秒）
        var delaySeconds = Random.Shared.Next(5, 11);
        await Task.Delay(TimeSpan.FromSeconds(delaySeconds));

        // ダミーの気温データを生成する
        var temperature = Random.Shared.Next(-5, 35);
        logger.LogInformation("{City}: {Temp}°C (took {Delay}s)", city, temperature, delaySeconds);

        // 都市名と気温を 1 件の結果として返す
        return new CityWeather(city, temperature);
    }
}
