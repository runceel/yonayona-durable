using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;

namespace Functions;

public static class StartCreation
{
    [Function(nameof(StartCreation))]
    public static async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", "get")] HttpRequestData req,
        [DurableClient] DurableTaskClient client,
        FunctionContext context)
    {
        var logger = context.GetLogger(nameof(StartCreation));

        // リソース作成オーケストレーションを新規開始する。
        string instanceId = await client.ScheduleNewOrchestrationInstanceAsync(
            nameof(ResourceCreationOrchestrator));

        logger.LogInformation("オーケストレーションを開始しました。InstanceId = {InstanceId}", instanceId);

        return await client.CreateCheckStatusResponseAsync(req, instanceId);
    }
}
