using System.Text;
using EngineeringInsights.DependencyAnalyzer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace EngineeringInsights.WorkerApp.Functions;

/// <summary>
/// HTTP trigger that exports the dependency graph of all packages as a downloadable Mermaid file.
/// </summary>
public class ExportDependencyGraphFunction(IDiagramGeneratorService diagramGenerator, ILogger<ExportDependencyGraphFunction> logger)
{
    [Function(nameof(ExportDependencyGraphFunction))]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "export-dependency-graph")] HttpRequest request)
    {
        logger.LogInformation("Dependency graph export requested.");

        var diagram = await diagramGenerator.GenerateDiagramAsync();

        return new FileContentResult(Encoding.UTF8.GetBytes(diagram), "text/plain; charset=utf-8")
        {
            FileDownloadName = "dependencies.mmd"
        };
    }
}
