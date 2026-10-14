using EngineeringInsights.Dal;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace EngineeringInsights.DependencyAnalyzer.Tests;

internal sealed class SqliteTestDatabase : IDisposable
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    public SqliteTestDatabase()
    {
        _connection.Open();

        DbContext = new EngineeringInsightsDbContext(
            new DbContextOptionsBuilder<EngineeringInsightsDbContext>().UseSqlite(_connection).Options);
        DbContext.Database.EnsureCreated();
    }

    public EngineeringInsightsDbContext DbContext { get; }

    public void Dispose()
    {
        DbContext.Dispose();
        _connection.Dispose();
    }
}
