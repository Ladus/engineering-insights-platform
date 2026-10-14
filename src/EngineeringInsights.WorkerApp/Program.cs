using Azure.Monitor.OpenTelemetry.Exporter;
using EngineeringInsights.Dal;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Azure.Functions.Worker.OpenTelemetry;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

var databasePath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../engineering-insights.db"));
builder.Services.AddDbContext<EngineeringInsightsDbContext>(options => options.UseSqlite($"Data Source={databasePath}"));

if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING")))
{
    builder.Services.AddOpenTelemetry()
        .UseFunctionsWorkerDefaults()
        .UseAzureMonitorExporter();
}

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<EngineeringInsightsDbContext>();
    dbContext.Database.Migrate();
}

app.Run();
