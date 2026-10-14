using Microsoft.EntityFrameworkCore;

namespace EngineeringInsights.Dal;

public sealed class EngineeringInsightsDbContext(DbContextOptions<EngineeringInsightsDbContext> options)
    : DbContext(options)
{
    public DbSet<ComponentConfiguration> ComponentConfigurations => Set<ComponentConfiguration>();

    public DbSet<PackageDependency> PackageDependencies => Set<PackageDependency>();
}