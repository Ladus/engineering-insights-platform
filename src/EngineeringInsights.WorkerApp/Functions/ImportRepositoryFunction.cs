using EngineeringInsights.DependencyAnalyzer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace EngineeringInsights.WorkerApp.Functions;

/// <summary>
/// HTTP trigger that imports a single repository on demand. Takes the <c>repositoryName</c> route parameter and returns 404 for an unknown repository.
/// </summary>
public class ImportRepositoryFunction(IImportRepositoryService importRepositoryService, ILogger<ImportRepositoryFunction> logger)
{
    [Function(nameof(ImportRepositoryFunction))]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "import-repository/{repositoryName}")] HttpRequest request,
        string repositoryName)
    {
        logger.LogInformation("Import requested for repository {RepositoryName}.", repositoryName);

        try
        {
            await importRepositoryService.ImportRepositoryAsync(repositoryName);
        }
        catch (KeyNotFoundException)
        {
            return new NotFoundObjectResult($"Repository '{repositoryName}' was not found.");
        }

        return new OkResult();
    }
}
