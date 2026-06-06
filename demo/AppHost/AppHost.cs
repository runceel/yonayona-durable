using Microsoft.Extensions.DependencyInjection;

var builder = DistributedApplication.CreateBuilder(args);

#pragma warning disable ASPIREDURABLETASK001 // Experimental API

var storage = builder.AddAzureStorage("storage")
    .RunAsEmulator();

var scheduler = builder.AddDurableTaskScheduler("scheduler")
    .RunAsEmulator();

var taskHub = scheduler.AddTaskHub("taskhub");

// DTS Emulator のダッシュボード SPA は ?endpoint=<gRPC URL> 付きで
// /subscriptions/<sub>/schedulers/<sch>/taskhubs/<hub> を開いたときだけ
// 自動でエンドポイントを localStorage に登録してくれる仕様。
// Aspire 標準の Task Hub Dashboard リンクには endpoint クエリが付かず、
// 開いてもエンドポイント追加画面に飛ばされるため、自前のリンクを追加する。
// Aspire の AddTaskHub は OnResourceReady で Urls を完全に上書きするため、
// その後で実行されるよう OnResourceReady のコールバックで append する。
taskHub.OnResourceReady(async (resource, evt, ct) =>
{
    var dashboardEndpoint = scheduler.Resource.GetEndpoint("dashboard");
    var grpcEndpoint = scheduler.Resource.GetEndpoint("grpc");
    if (!dashboardEndpoint.IsAllocated || !grpcEndpoint.IsAllocated)
    {
        return;
    }

    var hubName = resource.Name;
    var grpcUrl = Uri.EscapeDataString(grpcEndpoint.Url);
    var deepLink = $"{dashboardEndpoint.Url}/subscriptions/default/schedulers/default/taskhubs/{hubName}?endpoint={grpcUrl}";

    var notifications = evt.Services.GetRequiredService<ResourceNotificationService>();
    await notifications.PublishUpdateAsync(resource, snapshot => snapshot with
    {
        Urls = [.. snapshot.Urls, new UrlSnapshot("auto-connect", deepLink, IsInternal: false)
        {
            DisplayProperties = new UrlDisplayPropertiesSnapshot("Task Hub Dashboard (auto-connect)", 0),
        }],
    }).ConfigureAwait(false);
});

builder.AddAzureFunctionsProject<Projects.Functions>("funcapp")
    .WithHostStorage(storage)
    .WithReference(taskHub)
    .WaitFor(scheduler)
    .WithExternalHttpEndpoints();

#pragma warning restore ASPIREDURABLETASK001

builder.Build().Run();
