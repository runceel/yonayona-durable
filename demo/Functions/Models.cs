namespace Functions;

/// <summary>
/// 都市の天気情報を表すモデル
/// </summary>
/// <param name="City">都市名</param>
/// <param name="Temperature">気温</param>
public record CityWeather(string City, int Temperature);

/// <summary>
/// 全都市の天気情報と平均気温をまとめたモデル
/// </summary>
/// <param name="Cities">都市の天気情報の配列</param>
/// <param name="Average">平均気温</param>
public record WeatherResult(CityWeather[] Cities, double Average);
