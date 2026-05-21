using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;

namespace Functions;

public static class StartWeather
{
    [Function(nameof(StartWeather))]
    public static async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", "get")] HttpRequestData req,
        [DurableClient] DurableTaskClient client,
        FunctionContext context)
    {
        var logger = context.GetLogger(nameof(StartWeather));

        string instanceId = await client.ScheduleNewOrchestrationInstanceAsync(
            nameof(WeatherOrchestrator));

        logger.LogInformation("Started orchestration with instance ID = {InstanceId}", instanceId);

        return await client.CreateCheckStatusResponseAsync(req, instanceId);
    }
}
