using EngineeringInsights.GitRepositoryClient.Mock.Generation;

namespace EngineeringInsights.GitRepositoryClient.Mock;

/// <summary>
/// Deterministic, in-memory <see cref="IGitRepositoryClient"/> that mimics an Azure DevOps organization.
/// It generates three tiers of .csproj files from a fixed seed: platform packages that every team uses,
/// shared packages of product teams (such as Client and Contracts) that are used by their own applications and by other teams,
/// and product applications (such as Api and App) that nothing references.
/// </summary>
public sealed class GitRepositoryMockClient : IGitRepositoryClient
{
    /// <summary>
    /// The git repositories, this initializes the in-memory mock repository upon creation of the client.
    /// </summary>
    private readonly IReadOnlyList<MockRepository> _repositories = MockRepositoryCatalog.Create();

    public Task<IEnumerable<(string AdoProject, string RepoName)>> GetRepositoriesAsync()
    {
        IEnumerable<(string AdoProject, string RepoName)> result = _repositories
            .Select(r => (r.AdoProject, r.Name))
            .ToList();

        return Task.FromResult(result);
    }

    public async IAsyncEnumerable<GitFileDto> GetFilesAsync(string repositoryName, string? fileNameContains = null)
    {
        var repository = FindRepository(repositoryName);

        var paths = repository.FilePaths
            .Where(path => fileNameContains is null || Path.GetFileName(path).Contains(fileNameContains, StringComparison.OrdinalIgnoreCase));

        foreach (var path in paths)
        {
            yield return new GitFileDto(path, repository.Name, repository.AdoProject);
        }
    }

    public Task<string> GetFileContentAsync(string repositoryName, string path)
        => Task.FromResult(FindRepository(repositoryName).GetFileContent(path));

    public Task<string?> GetOwnerTeamAsync(string repositoryName)
        => Task.FromResult(FindRepository(repositoryName).OwnerTeam);

    private MockRepository FindRepository(string repositoryName)
        => _repositories.FirstOrDefault(r => string.Equals(r.Name, repositoryName, StringComparison.OrdinalIgnoreCase))
            ?? throw new KeyNotFoundException($"Repository '{repositoryName}' was not found.");
}
