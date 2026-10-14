using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace EngineeringInsights.WorkerApp.Functions;

public class RefreshRepositoryFunction(ILogger<RefreshRepositoryFunction> logger)
{
    [Function(nameof(RefreshRepositoryFunction))]
    public IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "repositories/{repositoryName}/refresh")] HttpRequest request,
        string repositoryName)
    {
        logger.LogInformation("Refresh requested for repository {RepositoryName}.", repositoryName);

        return new OkResult();
    }
}
