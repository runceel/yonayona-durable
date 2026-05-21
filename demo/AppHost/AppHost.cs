var builder = DistributedApplication.CreateBuilder(args);

#pragma warning disable ASPIREDURABLETASK001 // Experimental API

var storage = builder.AddAzureStorage("storage")
    .RunAsEmulator();

var scheduler = builder.AddDurableTaskScheduler("scheduler")
    .RunAsEmulator();

var taskHub = scheduler.AddTaskHub("taskhub");

builder.AddAzureFunctionsProject<Projects.Functions>("funcapp")
    .WithHostStorage(storage)
    .WithReference(taskHub)
    .WaitFor(scheduler);

#pragma warning restore ASPIREDURABLETASK001

builder.Build().Run();
