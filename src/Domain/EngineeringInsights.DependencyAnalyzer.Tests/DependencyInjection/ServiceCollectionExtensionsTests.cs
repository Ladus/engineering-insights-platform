using EngineeringInsights.Dal;
using EngineeringInsights.DependencyAnalyzer.DependencyInjection;
using EngineeringInsights.DependencyAnalyzer.Services;
using EngineeringInsights.GitRepositoryClient;
using EngineeringInsights.GitRepositoryClient.Mock;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EngineeringInsights.DependencyAnalyzer.Tests.DependencyInjection;

public class ServiceCollectionExtensionsTests
{
    private static ServiceProvider BuildProvider()
    {
        var services = new ServiceCollection();
        services.AddDbContext<EngineeringInsightsDbContext>(options => options.UseSqlite("DataSource=:memory:"));
        services.AddDomainServices();

        return services.BuildServiceProvider(validateScopes: true);
    }

    [Fact]
    public void AddDomainServices_RegistersImportRepositoryServiceAsScoped()
    {
        // Arrange
        using var provider = BuildProvider();

        // Act
        using var firstScope = provider.CreateScope();
        using var secondScope = provider.CreateScope();
        var first = firstScope.ServiceProvider.GetRequiredService<IImportRepositoryService>();
        var firstAgain = firstScope.ServiceProvider.GetRequiredService<IImportRepositoryService>();
        var second = secondScope.ServiceProvider.GetRequiredService<IImportRepositoryService>();

        // Assert
        Assert.IsType<ImportRepositoryService>(first);
        Assert.Same(first, firstAgain);
        Assert.NotSame(first, second);
    }

    [Fact]
    public void AddDomainServices_RegistersDiagramGeneratorServiceAsScopedMermaidImplementation()
    {
        // Arrange
        using var provider = BuildProvider();

        // Act
        using var firstScope = provider.CreateScope();
        using var secondScope = provider.CreateScope();
        var first = firstScope.ServiceProvider.GetRequiredService<IDiagramGeneratorService>();
        var firstAgain = firstScope.ServiceProvider.GetRequiredService<IDiagramGeneratorService>();
        var second = secondScope.ServiceProvider.GetRequiredService<IDiagramGeneratorService>();

        // Assert
        Assert.IsType<MermaidDiagramGeneratorService>(first);
        Assert.Same(first, firstAgain);
        Assert.NotSame(first, second);
    }

    [Fact]
    public void AddDomainServices_RegistersCsProjParserServiceAsScoped()
    {
        // Arrange
        using var provider = BuildProvider();

        // Act
        using var firstScope = provider.CreateScope();
        using var secondScope = provider.CreateScope();
        var first = firstScope.ServiceProvider.GetRequiredService<ICsProjParserService>();
        var firstAgain = firstScope.ServiceProvider.GetRequiredService<ICsProjParserService>();
        var second = secondScope.ServiceProvider.GetRequiredService<ICsProjParserService>();

        // Assert
        Assert.IsType<CsProjParserService>(first);
        Assert.Same(first, firstAgain);
        Assert.NotSame(first, second);
    }

    [Fact]
    public void AddDomainServices_RegistersGitRepositoryClientAsScoped()
    {
        // Arrange
        using var provider = BuildProvider();

        // Act
        using var firstScope = provider.CreateScope();
        using var secondScope = provider.CreateScope();
        var first = firstScope.ServiceProvider.GetRequiredService<IGitRepositoryClient>();
        var firstAgain = firstScope.ServiceProvider.GetRequiredService<IGitRepositoryClient>();
        var second = secondScope.ServiceProvider.GetRequiredService<IGitRepositoryClient>();

        // Assert
        Assert.IsType<GitRepositoryMockClient>(first);
        Assert.Same(first, firstAgain);
        Assert.NotSame(first, second);
    }
}
