namespace EngineeringInsights.GitRepositoryClient;

public interface IGitRepositoryClient
{
    /// <summary>
    /// Get all repositories and their associated Azure DevOps project names.
    /// </summary>
    /// <returns></returns>
    Task<IEnumerable<(string AdoProject, string RepoName)>> GetRepositoriesAsync();

    /// <summary>
    /// Streams the files of the repository.
    /// </summary>
    /// <param name="repositoryName">The name of the repository.</param>
    /// <param name="fileNameContains">
    /// Optional text the file name must contain (for example <c>.csproj</c>), compared case-insensitively.
    /// </param>
    IAsyncEnumerable<GitFileDto> GetFilesAsync(string repositoryName, string? fileNameContains = null);

    /// <summary>
    /// Get the contents of a specific file from the git repository.
    /// </summary>
    Task<string> GetFileContentAsync(string repositoryName, string path);

    /// <summary>
    /// Returns null when the owner team cannot be determined from the repository.
    /// </summary>
    Task<string?> GetOwnerTeamAsync(string repositoryName);
}
