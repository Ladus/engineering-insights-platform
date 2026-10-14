using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace EngineeringInsights.WorkerApp.Functions;

public class ExportDependencyGraphFunction(ILogger<ExportDependencyGraphFunction> logger)
{
    [Function(nameof(ExportDependencyGraphFunction))]
    public IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "dependencies/graph")] HttpRequest request)
    {
        logger.LogInformation("Dependency graph export requested.");

        return new OkResult();
    }
}
