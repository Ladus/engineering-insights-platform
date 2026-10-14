using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace EngineeringInsights.WorkerApp.Functions;

public class WeeklyInventorySweepFunction(ILogger<WeeklyInventorySweepFunction> logger)
{
    [Function(nameof(WeeklyInventorySweepFunction))]
    public void Run([TimerTrigger("0 0 0 * * 0")] TimerInfo timer)
    {
        logger.LogInformation("Weekly inventory sweep triggered.");
    }
}
