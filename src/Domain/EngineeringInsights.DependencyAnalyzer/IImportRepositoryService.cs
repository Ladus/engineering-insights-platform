namespace EngineeringInsights.DependencyAnalyzer;

public interface IImportRepositoryService
{
    /// <summary>Imports every repository and removes data of repositories that no longer exist.</summary>
    Task ImportAllRepositoriesAsync();

    /// <summary>Purges and re-imports the stored data of a single repository.</summary>
    Task ImportRepositoryAsync(string repositoryName);
}
