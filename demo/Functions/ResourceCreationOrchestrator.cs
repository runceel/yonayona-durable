using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;

namespace Functions;

public static class ResourceCreationOrchestrator
{
    public const string HumanApprovalEventName = "HumanApproval";

    [Function(nameof(ResourceCreationOrchestrator))]
    public static async Task<CreationWorkflowResult> RunOrchestrator(
        [OrchestrationTrigger] TaskOrchestrationContext context)
    {
        var logger = context.CreateReplaySafeLogger(nameof(ResourceCreationOrchestrator));

        logger.LogInformation("リソース作成オーケストレーションを開始します。");

        // Tokyo → Seattle → London の順に Activity 関数を実行する。
        var tokyo = await context.CallActivityAsync<LocationCreationResult>(
            nameof(ResourceCreationActivities.CreateResource),
            "Tokyo");
        logger.LogInformation("Tokyo のリソース作成が完了しました。");

        var seattle = await context.CallActivityAsync<LocationCreationResult>(
            nameof(ResourceCreationActivities.CreateResource),
            "Seattle");
        logger.LogInformation("Seattle のリソース作成が完了しました。");

        var london = await context.CallActivityAsync<LocationCreationResult>(
            nameof(ResourceCreationActivities.CreateResource),
            "London");
        logger.LogInformation("London のリソース作成が完了しました。");

        LocationCreationResult[] results = [tokyo, seattle, london];

        // 3 つの作成結果がそろったら、人の判断待ちであることを状態に出す。
        context.SetCustomStatus(new CreationApprovalStatus(
            "WaitingForHumanApproval",
            HumanApprovalEventName,
            results));

        logger.LogInformation("3 拠点の作成が完了したため、人の承認待ちに入ります。イベント名: {EventName}", HumanApprovalEventName);

        // HTTP トリガーから送られる OK / NG の外部イベントを待つ。
        var approval = await context.WaitForExternalEvent<ApprovalDecision>(
            HumanApprovalEventName);

        logger.LogInformation(
            "承認イベントを受信しました。承認結果: {IsApproved}",
            approval.IsApproved);

        if (approval.IsApproved)
        {
            logger.LogInformation("承認されたため、作成結果を返して終了します。");
            return new CreationWorkflowResult(
                "Approved",
                "承認されました。",
                results);
        }

        // 本来は拒否された場合のクリーンアップ処理などを入れるべきだが、ここでは省略する。
        logger.LogInformation("拒否されたため、空の結果で終了します。");
        return new CreationWorkflowResult(
            "Rejected",
            "拒否されました。",
            []);
    }
}
