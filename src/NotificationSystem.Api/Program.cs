using NotificationSystem.Api;
using NotificationSystem.Api.Configuration;
using Serilog;

try
{
    Log.Information("Starting web application");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.ConfigureLogging();
    builder.Services.ConfigureApp(builder.Configuration);

    var app = builder.Build();

    await app.ConfigureExtensions();
    app.MapControllers();

    await app.RunAsync().ConfigureAwait(false);
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
