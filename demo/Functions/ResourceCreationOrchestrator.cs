using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;

namespace Functions;

public static class ResourceCreationOrchestrator
{
    public const string HumanApprovalEventName = "HumanApproval";

    [Function(nameof(ResourceCreationOrchestrator))]
    public static async Task<CreationWorkflowResult> RunOrchestrator(
        [OrchestrationTrigger] TaskOrchestrationContext context)
    {
        // Tokyo → Seattle → London の順に Activity 関数を実行する。
        var tokyo = await context.CallActivityAsync<LocationCreationResult>(
            nameof(ResourceCreationActivities.CreateResource),
            "Tokyo");

        var seattle = await context.CallActivityAsync<LocationCreationResult>(
            nameof(ResourceCreationActivities.CreateResource),
            "Seattle");

        var london = await context.CallActivityAsync<LocationCreationResult>(
            nameof(ResourceCreationActivities.CreateResource),
            "London");

        LocationCreationResult[] results = [tokyo, seattle, london];

        // 3 つの作成結果がそろったら、人の判断待ちであることを状態に出す。
        context.SetCustomStatus(new CreationApprovalStatus(
            "WaitingForHumanApproval",
            HumanApprovalEventName,
            results));

        // HTTP トリガーから送られる OK / NG の外部イベントを待つ。
        var approval = await context.WaitForExternalEvent<ApprovalDecision>(
            HumanApprovalEventName);

        if (approval.Decision == ApprovalDecisions.Ok)
        {
            return new CreationWorkflowResult(
                "Approved",
                "承認されました。",
                results);
        }

        return new CreationWorkflowResult(
            "Rejected",
            "拒否されました。",
            []);
    }
}
