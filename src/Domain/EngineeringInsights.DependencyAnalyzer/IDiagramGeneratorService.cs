namespace EngineeringInsights.DependencyAnalyzer;

public interface IDiagramGeneratorService
{
    /// <summary>
    /// Generates a diagram of the packages published by projects in our repositories and the references between them.
    /// External packages (without a project of our own) are left out.
    /// Arrows point from a package to the packages that use it.
    /// </summary>
    Task<string> GenerateDiagramAsync();

    /// <summary>
    /// Generates a diagram of a single package: its transitive dependencies and its transitive dependents.
    /// Throws <see cref="KeyNotFoundException"/> when the package is not published by a project in our repositories.
    /// </summary>
    Task<string> GenerateDiagramAsync(string packageId);
}
