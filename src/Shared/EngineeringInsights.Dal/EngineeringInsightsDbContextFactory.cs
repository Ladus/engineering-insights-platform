using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EngineeringInsights.Dal;

public sealed class EngineeringInsightsDbContextFactory : IDesignTimeDbContextFactory<EngineeringInsightsDbContext>
{
    public EngineeringInsightsDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<EngineeringInsightsDbContext>();
        optionsBuilder.UseSqlite("Data Source=engineering-insights.db");

        return new EngineeringInsightsDbContext(optionsBuilder.Options);
    }
}