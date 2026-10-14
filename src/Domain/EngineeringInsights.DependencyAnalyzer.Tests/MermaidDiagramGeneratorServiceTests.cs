using EngineeringInsights.Dal;

namespace EngineeringInsights.DependencyAnalyzer.Tests;

public sealed class MermaidDiagramGeneratorServiceTests : IDisposable
{
    private readonly SqliteTestDatabase _database = new();
    private readonly MermaidDiagramGeneratorService _generator;

    public MermaidDiagramGeneratorServiceTests()
    {
        _generator = new MermaidDiagramGeneratorService(_database.DbContext);
    }

    public void Dispose() => _database.Dispose();

    [Fact]
    public async Task GenerateDiagramAsync_WithoutData_ReturnsEmptyGraph()
    {
        // Arrange

        // Act
        var diagram = await _generator.GenerateDiagramAsync();

        // Assert
        Assert.Equal("graph TD\n", diagram);
    }

    [Fact]
    public async Task GenerateDiagramAsync_ReturnsOnlyPublishedPackagesAndTheirReferences()
    {
        // Arrange
        await SeedAsync(
            ("Lib", ["Newtonsoft.Json"]),
            ("App", ["Lib", "Newtonsoft.Json"]));

        // Act
        var diagram = await _generator.GenerateDiagramAsync();

        // Assert
        var expected = string.Join('\n',
            "graph TD",
            "    n0[\"App\"]",
            "    n1[\"Lib\"]",
            "    n1 --> n0",
            string.Empty);
        Assert.Equal(expected, diagram);
    }

    [Fact]
    public async Task GenerateDiagramAsync_IncludesPublishedPackagesWithoutDependencies()
    {
        // Arrange
        await SeedAsync(("Lonely", []));

        // Act
        var diagram = await _generator.GenerateDiagramAsync();

        // Assert
        Assert.Equal("graph TD\n    n0[\"Lonely\"]\n", diagram);
    }

    [Fact]
    public async Task GenerateDiagramAsync_ForPackage_ReturnsTransitiveDependenciesAndDependentsOnly()
    {
        // Arrange
        await SeedAsync(
            ("A", ["B"]),
            ("B", ["C"]),
            ("C", []),
            ("D", ["A"]),
            ("E", ["C"]),
            ("F", ["G"]));

        // Act
        var diagram = await _generator.GenerateDiagramAsync("A");

        // Assert
        var expected = string.Join('\n',
            "graph TD",
            "    n0[\"A\"]",
            "    n1[\"B\"]",
            "    n2[\"C\"]",
            "    n3[\"D\"]",
            "    n0 --> n3",
            "    n1 --> n0",
            "    n2 --> n1",
            string.Empty);
        Assert.Equal(expected, diagram);
    }

    [Fact]
    public async Task GenerateDiagramAsync_ForPackage_IgnoresPackageIdCasing()
    {
        // Arrange
        await SeedAsync(
            ("Lib", []),
            ("App", ["lib"]));

        // Act
        var diagram = await _generator.GenerateDiagramAsync("LIB");

        // Assert
        Assert.Equal("graph TD\n    n0[\"App\"]\n    n1[\"Lib\"]\n    n1 --> n0\n", diagram);
    }

    [Fact]
    public async Task GenerateDiagramAsync_ForPackageWithoutConnections_ReturnsSingleNode()
    {
        // Arrange
        await SeedAsync(
            ("Lonely", []),
            ("Other", ["Serilog"]));

        // Act
        var diagram = await _generator.GenerateDiagramAsync("Lonely");

        // Assert
        Assert.Equal("graph TD\n    n0[\"Lonely\"]\n", diagram);
    }

    [Fact]
    public async Task GenerateDiagramAsync_ForExternalPackage_Throws()
    {
        // Arrange
        await SeedAsync(("App", ["Serilog"]));

        // Act
        var act = () => _generator.GenerateDiagramAsync("Serilog");

        // Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(act);
    }

    [Fact]
    public async Task GenerateDiagramAsync_ForUnknownPackage_Throws()
    {
        // Arrange
        await SeedAsync(("Lib", []));

        // Act
        var act = () => _generator.GenerateDiagramAsync("Missing");

        // Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(act);
    }

    private async Task SeedAsync(params (string PackageId, string[] References)[] packages)
    {
        foreach (var (packageId, references) in packages)
        {
            _database.DbContext.ComponentConfigurations.Add(new ComponentConfiguration
            {
                AdoProjectName = "Platform",
                SourceRepositoryName = "platform-core",
                PublishedPackageId = packageId
            });

            foreach (var reference in references)
            {
                _database.DbContext.PackageDependencies.Add(new PackageDependency
                {
                    DependentPackageId = packageId,
                    TargetPackageId = reference
                });
            }
        }

        await _database.DbContext.SaveChangesAsync();
    }
}
