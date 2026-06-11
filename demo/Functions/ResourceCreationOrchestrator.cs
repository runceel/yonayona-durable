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
        // デモ対象の city はオーケストレーター内で固定する。
        string[] cities = ["Tokyo", "Seattle", "London"];

        List<LocationCreationResult> results = [];

        // Tokyo → Seattle → London の順に Activity 関数を実行する。
        foreach (var city in cities)
        {
            var result = await context.CallActivityAsync<LocationCreationResult>(
                nameof(ResourceCreationActivities.CreateResource),
                city);
            results.Add(result);
        }

        // 3 つの作成結果がそろったら、人の判断待ちであることを状態に出す。
        context.SetCustomStatus(new CreationApprovalStatus(
            "WaitingForHumanApproval",
            HumanApprovalEventName,
            [.. results]));

        // HTTP トリガーから送られる OK / NG の外部イベントを待つ。
        var approval = await context.WaitForExternalEvent<ApprovalDecision>(
            HumanApprovalEventName);

        if (approval.Decision == ApprovalDecisions.Ok)
        {
            return new CreationWorkflowResult(
                "Approved",
                "Human approval was accepted.",
                [.. results]);
        }

        return new CreationWorkflowResult(
            "Rejected",
            "Human approval was rejected.",
            []);
    }
}
