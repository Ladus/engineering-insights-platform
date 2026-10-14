using EngineeringInsights.DependencyAnalyzer.Services;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace EngineeringInsights.DependencyAnalyzer.Tests;

public sealed class ImportRepositoryServiceTests : IDisposable
{
    private readonly SqliteTestDatabase _database = new();
    private readonly GitRepositoryClientFixture _git = new();
    private readonly ImportRepositoryService _service;

    public ImportRepositoryServiceTests()
    {
        _service = new ImportRepositoryService(_git.Client, new CsProjParserService(), _database.DbContext);
    }

    public void Dispose() => _database.Dispose();

    [Fact]
    public async Task ImportRepositoryAsync_StoresComponentsAndDependencies()
    {
        // Arrange
        _git.SetRepository("Platform", "platform-core", "Team Alpha",
            ("src/Core/Core.csproj", CsprojContent.Create("Contoso.Core", "Newtonsoft.Json")),
            ("src/App/App.csproj", CsprojContent.Create(null, "Contoso.Core")));

        // Act
        await _service.ImportRepositoryAsync("platform-core");

        // Assert
        var components = await _database.DbContext.ComponentConfigurations.OrderBy(c => c.PublishedPackageId).ToListAsync();
        Assert.Equal(["App", "Contoso.Core"], components.Select(c => c.PublishedPackageId));
        Assert.All(components, c =>
        {
            Assert.Equal("Platform", c.AdoProjectName);
            Assert.Equal("platform-core", c.SourceRepositoryName);
            Assert.Equal("Team Alpha", c.OwnerTeam);
        });

        var dependencies = await _database.DbContext.PackageDependencies
            .OrderBy(d => d.DependentPackageId).ThenBy(d => d.TargetPackageId)
            .Select(d => $"{d.DependentPackageId}->{d.TargetPackageId}")
            .ToListAsync();
        Assert.Equal(["App->Contoso.Core", "Contoso.Core->Newtonsoft.Json"], dependencies);
    }

    [Fact]
    public async Task ImportRepositoryAsync_WithoutOwnerTeam_StoresNullOwner()
    {
        // Arrange
        _git.SetRepository("Platform", "platform-core", null,
            ("src/Core/Core.csproj", CsprojContent.Create("Contoso.Core")));

        // Act
        await _service.ImportRepositoryAsync("platform-core");

        // Assert
        var component = await _database.DbContext.ComponentConfigurations.SingleAsync();
        Assert.Null(component.OwnerTeam);
    }

    [Fact]
    public async Task ImportRepositoryAsync_IgnoresFilesThatAreNotProjectFiles()
    {
        // Arrange
        _git.SetRepository("Platform", "platform-core", "Team Alpha",
            ("README.md", "# platform-core"),
            ("src/Core/Class1.cs", "namespace Core;"),
            ("src/Core/Core.csproj", CsprojContent.Create("Contoso.Core", "Newtonsoft.Json")),
            ("src/Tools/Tools.CSPROJ", CsprojContent.Create("Contoso.Tools")));

        // Act
        await _service.ImportRepositoryAsync("platform-core");

        // Assert
        var packageIds = await _database.DbContext.ComponentConfigurations
            .OrderBy(c => c.PublishedPackageId)
            .Select(c => c.PublishedPackageId)
            .ToListAsync();
        Assert.Equal(["Contoso.Core", "Contoso.Tools"], packageIds);
    }

    [Fact]
    public async Task ImportRepositoryAsync_ProcessesFilesWhileStreaming()
    {
        // Arrange
        _git.SetRepository("Platform", "platform-core", null,
            ("README.md", "# platform-core"),
            ("src/A/A.csproj", CsprojContent.Create("A")),
            ("src/B/B.csproj", CsprojContent.Create("B")));

        // Act
        await _service.ImportRepositoryAsync("platform-core");

        // Assert
        Assert.Equal(
            [
                "file:src/A/A.csproj",
                "content:src/A/A.csproj",
                "file:src/B/B.csproj",
                "content:src/B/B.csproj"
            ],
            _git.Events);
    }

    [Fact]
    public async Task ImportRepositoryAsync_RequestsOnlyProjectFilesFromTheClient()
    {
        // Arrange
        _git.SetRepository("Platform", "platform-core", null,
            ("src/Core/Core.csproj", CsprojContent.Create("Contoso.Core")));

        // Act
        await _service.ImportRepositoryAsync("platform-core");

        // Assert
        _git.ClientMock.Verify(c => c.GetFilesAsync("platform-core", ".csproj"), Times.Once);
    }

    [Fact]
    public async Task ImportRepositoryAsync_IgnoresFilesThatOnlyContainTheProjectSuffix()
    {
        // Arrange
        _git.SetRepository("Platform", "platform-core", null,
            ("src/Core/Core.csproj", CsprojContent.Create("Contoso.Core")),
            ("src/Core/Core.csproj.user", CsprojContent.Create("Contoso.User")));

        // Act
        await _service.ImportRepositoryAsync("platform-core");

        // Assert
        var packageIds = await _database.DbContext.ComponentConfigurations.Select(c => c.PublishedPackageId).ToListAsync();
        Assert.Equal(["Contoso.Core"], packageIds);
    }

    [Fact]
    public async Task ImportRepositoryAsync_ReplacesOnlyDataOfThatRepository()
    {
        // Arrange
        _git.SetRepository("Platform", "platform-core", "Team Alpha",
            ("src/Core/Core.csproj", CsprojContent.Create("Contoso.Core", "Newtonsoft.Json")));
        _git.SetRepository("Payments", "payments-api", "Team Bravo",
            ("src/Api/Api.csproj", CsprojContent.Create("Contoso.Api", "Contoso.Core")));
        await _service.ImportAllRepositoriesAsync();

        _git.SetRepository("Platform", "platform-core", "Team Charlie",
            ("src/Core/Core.csproj", CsprojContent.Create("Contoso.Core", "Serilog")));

        // Act
        await _service.ImportRepositoryAsync("platform-core");

        // Assert
        var components = await _database.DbContext.ComponentConfigurations.OrderBy(c => c.PublishedPackageId).ToListAsync();
        Assert.Equal(["Contoso.Api", "Contoso.Core"], components.Select(c => c.PublishedPackageId));
        Assert.Equal("Team Bravo", components.Single(c => c.PublishedPackageId == "Contoso.Api").OwnerTeam);
        Assert.Equal("Team Charlie", components.Single(c => c.PublishedPackageId == "Contoso.Core").OwnerTeam);

        var dependencies = await _database.DbContext.PackageDependencies
            .OrderBy(d => d.DependentPackageId)
            .Select(d => $"{d.DependentPackageId}->{d.TargetPackageId}")
            .ToListAsync();
        Assert.Equal(["Contoso.Api->Contoso.Core", "Contoso.Core->Serilog"], dependencies);
    }

    [Fact]
    public async Task ImportRepositoryAsync_IgnoresRepositoryNameCasing()
    {
        // Arrange
        _git.SetRepository("Platform", "platform-core", "Team Alpha",
            ("src/Core/Core.csproj", CsprojContent.Create("Contoso.Core")));
        await _service.ImportRepositoryAsync("platform-core");

        // Act
        await _service.ImportRepositoryAsync("PLATFORM-CORE");

        // Assert
        Assert.Equal(1, await _database.DbContext.ComponentConfigurations.CountAsync());
    }

    [Fact]
    public async Task ImportRepositoryAsync_WhenClientFails_KeepsExistingData()
    {
        // Arrange
        _git.SetRepository("Platform", "platform-core", "Team Alpha",
            ("src/Core/Core.csproj", CsprojContent.Create("Contoso.Core")));
        await _service.ImportRepositoryAsync("platform-core");
        _git.RemoveRepository("platform-core");

        // Act
        var act = () => _service.ImportRepositoryAsync("platform-core");

        // Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(act);
        Assert.Equal(1, await _database.DbContext.ComponentConfigurations.CountAsync());
    }

    [Fact]
    public async Task ImportAllRepositoriesAsync_ImportsAllRepositoriesAndRemovesMissingOnes()
    {
        // Arrange
        _git.SetRepository("Platform", "platform-core", "Team Alpha",
            ("src/Core/Core.csproj", CsprojContent.Create("Contoso.Core", "Newtonsoft.Json")));
        _git.SetRepository("Payments", "payments-api", "Team Bravo",
            ("src/Api/Api.csproj", CsprojContent.Create("Contoso.Api", "Contoso.Core")));
        await _service.ImportAllRepositoriesAsync();

        _git.RemoveRepository("payments-api");
        _git.SetRepository("Logistics", "logistics-worker", null,
            ("src/Worker/Worker.csproj", CsprojContent.Create("Contoso.Worker")));

        // Act
        await _service.ImportAllRepositoriesAsync();

        // Assert
        var components = await _database.DbContext.ComponentConfigurations.OrderBy(c => c.PublishedPackageId).ToListAsync();
        Assert.Equal(["Contoso.Core", "Contoso.Worker"], components.Select(c => c.PublishedPackageId));

        var dependencies = await _database.DbContext.PackageDependencies
            .Select(d => $"{d.DependentPackageId}->{d.TargetPackageId}")
            .ToListAsync();
        Assert.Equal(["Contoso.Core->Newtonsoft.Json"], dependencies);
    }
}
