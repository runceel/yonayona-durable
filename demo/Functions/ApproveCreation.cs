using System.Net;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;

namespace Functions;

public static class ApproveCreation
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Function(nameof(ApproveCreation))]
    public static async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "ApproveCreation/{instanceId}")] HttpRequestData req,
        string instanceId,
        [DurableClient] DurableTaskClient client,
        FunctionContext context)
    {
        var logger = context.GetLogger(nameof(ApproveCreation));
        ApprovalDecisionRequest? approvalRequest;

        // リクエスト本文から OK / NG の判断を受け取る。
        try
        {
            approvalRequest = await JsonSerializer.DeserializeAsync<ApprovalDecisionRequest>(
                req.Body,
                JsonOptions);
        }
        catch (JsonException ex)
        {
            logger.LogWarning(ex, "承認リクエストの JSON が不正です");
            return await CreateBadRequestResponse(req, "リクエスト本文は JSON にしてください");
        }

        if (!ApprovalDecisions.TryNormalize(approvalRequest?.IsApproved, out var decision))
        {
            return await CreateBadRequestResponse(req, "decision は OK または NG を指定してください");
        }

        // 待機中のオーケストレーションへ HumanApproval 外部イベントを送る。
        await client.RaiseEventAsync(
            instanceId,
            ResourceCreationOrchestrator.HumanApprovalEventName,
            new ApprovalDecision(decision));

        logger.LogInformation(
            "オーケストレーション {InstanceId} に {EventName}={Decision} を送信しました",
            ResourceCreationOrchestrator.HumanApprovalEventName,
            decision,
            instanceId);

        var response = req.CreateResponse(HttpStatusCode.Accepted);
        await response.WriteAsJsonAsync(new
        {
            instanceId,
            eventName = ResourceCreationOrchestrator.HumanApprovalEventName,
            decision
        });
        response.StatusCode = HttpStatusCode.Accepted;
        return response;
    }

    private static async Task<HttpResponseData> CreateBadRequestResponse(
        HttpRequestData req,
        string error)
    {
        var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
        await badRequest.WriteAsJsonAsync(new
        {
            error
        });
        badRequest.StatusCode = HttpStatusCode.BadRequest;
        return badRequest;
    }
}
