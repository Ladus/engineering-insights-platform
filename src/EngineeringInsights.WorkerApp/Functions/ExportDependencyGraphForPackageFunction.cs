using System.Text;
using EngineeringInsights.DependencyAnalyzer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace EngineeringInsights.WorkerApp.Functions;

/// <summary>
/// HTTP trigger that exports the dependency graph of a single package, with its dependencies and dependents, as a downloadable Mermaid file.
/// Takes the <c>packageId</c> route parameter and returns 404 for an unknown package.
/// </summary>
public class ExportDependencyGraphForPackageFunction(IDiagramGeneratorService diagramGenerator, ILogger<ExportDependencyGraphForPackageFunction> logger)
{
    [Function(nameof(ExportDependencyGraphForPackageFunction))]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "export-dependency-graph-for-package/{packageId}")] HttpRequest request,
        string packageId)
    {
        logger.LogInformation("Dependency graph export requested for package {PackageId}.", packageId);

        string diagram;
        try
        {
            diagram = await diagramGenerator.GenerateDiagramAsync(packageId);
        }
        catch (KeyNotFoundException)
        {
            return new NotFoundObjectResult($"Package '{packageId}' was not found.");
        }

        return new FileContentResult(Encoding.UTF8.GetBytes(diagram), "text/plain; charset=utf-8")
        {
            FileDownloadName = $"{packageId}.mmd"
        };
    }
}
