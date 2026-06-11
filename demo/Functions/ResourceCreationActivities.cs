using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Functions;

public static class ResourceCreationActivities
{
    [Function(nameof(CreateResource))]
    public static async Task<LocationCreationResult> CreateResource(
        [ActivityTrigger] string city,
        FunctionContext context)
    {
        var logger = context.GetLogger(nameof(CreateResource));
        logger.LogInformation("Creating resource in {City}", city);

        // 実際のリソース作成処理の代わりに、5 秒かかる処理として見せる。
        await Task.Delay(TimeSpan.FromSeconds(5));

        var resourceName = $"demo-resource-{city.ToLowerInvariant()}";
        logger.LogInformation("Created {ResourceName} in {City}", resourceName, city);

        return new LocationCreationResult(
            city,
            resourceName,
            "Created",
            DateTimeOffset.UtcNow);
    }
}
