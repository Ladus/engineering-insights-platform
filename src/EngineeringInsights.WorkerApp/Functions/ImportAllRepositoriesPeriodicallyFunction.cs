using EngineeringInsights.DependencyAnalyzer;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace EngineeringInsights.WorkerApp.Functions;

/// <summary>
/// Timer trigger that imports all repositories every Sunday at midnight.
/// </summary>
public class ImportAllRepositoriesPeriodicallyFunction(IImportRepositoryService importRepositoryService, ILogger<ImportAllRepositoriesPeriodicallyFunction> logger)
{
    [Function("import-all-repositories-periodically")]
    public async Task Run([TimerTrigger("0 0 0 * * 0")] TimerInfo timer)
    {
        logger.LogInformation("Periodic import of all repositories started.");

        await importRepositoryService.ImportAllRepositoriesAsync();

        logger.LogInformation("Periodic import of all repositories completed.");
    }
}
