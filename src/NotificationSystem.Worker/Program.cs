using NotificationSystem.Worker;

var host = NotificationWorkerHost.CreateHost(args);
await NotificationWorkerHost.InitializeAsync(host);
await host.RunAsync();

public partial class Program;
