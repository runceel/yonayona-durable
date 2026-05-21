namespace Functions;

public record CityWeather(string City, int Temperature);

public record WeatherResult(CityWeather[] Cities, double Average);
