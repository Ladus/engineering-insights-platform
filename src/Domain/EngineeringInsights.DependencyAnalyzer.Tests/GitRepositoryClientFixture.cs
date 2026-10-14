using EngineeringInsights.GitRepositoryClient;
using Moq;

namespace EngineeringInsights.DependencyAnalyzer.Tests;

/// <summary>
/// Configures a <see cref="IGitRepositoryClient"/> mock that serves the repositories registered with <see cref="SetRepository"/>.
/// Create one per test, because the registered repositories and <see cref="Events"/> are mutable state.
/// </summary>
internal sealed class GitRepositoryClientFixture
{
    private readonly Dictionary<string, (string AdoProject, string Name, string? OwnerTeam, IReadOnlyList<(string Path, string Content)> Files)> _repositories
        = new(StringComparer.OrdinalIgnoreCase);

    public GitRepositoryClientFixture()
    {
        ClientMock.Setup(c => c.GetRepositoriesAsync())
            .Returns(() => Task.FromResult<IEnumerable<(string AdoProject, string RepoName)>>(
                _repositories.Values.Select(r => (r.AdoProject, r.Name)).ToList()));

        ClientMock.Setup(c => c.GetFilesAsync(It.IsAny<string>(), It.IsAny<string?>()))
            .Returns((string repositoryName, string? fileNameContains) => GetFilesAsync(repositoryName, fileNameContains));

        ClientMock.Setup(c => c.GetFileContentAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Returns((string repositoryName, string path) => Task.FromResult(GetFileContent(repositoryName, path)));

        ClientMock.Setup(c => c.GetOwnerTeamAsync(It.IsAny<string>()))
            .Returns((string repositoryName) => Task.FromResult(Find(repositoryName).OwnerTeam));
    }

    public Mock<IGitRepositoryClient> ClientMock { get; } = new();

    public IGitRepositoryClient Client => ClientMock.Object;

    /// <summary>The order in which files were streamed (<c>file:</c>) and their content was requested (<c>content:</c>).</summary>
    public List<string> Events { get; } = [];

    public void SetRepository(string adoProject, string name, string? ownerTeam, params (string Path, string Content)[] files)
        => _repositories[name] = (adoProject, name, ownerTeam, files);

    public void RemoveRepository(string name) => _repositories.Remove(name);

    private async IAsyncEnumerable<GitFileDto> GetFilesAsync(string repositoryName, string? fileNameContains)
    {
        var repository = Find(repositoryName);

        foreach (var (path, _) in repository.Files)
        {
            if (fileNameContains is not null && !Path.GetFileName(path).Contains(fileNameContains, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            await Task.Yield();
            Events.Add($"file:{path}");
            yield return new GitFileDto(path, repository.Name, repository.AdoProject);
        }
    }

    private string GetFileContent(string repositoryName, string path)
    {
        Events.Add($"content:{path}");

        return Find(repositoryName).Files.Single(f => f.Path == path).Content;
    }

    private (string AdoProject, string Name, string? OwnerTeam, IReadOnlyList<(string Path, string Content)> Files) Find(string repositoryName)
        => _repositories.TryGetValue(repositoryName, out var repository)
            ? repository
            : throw new KeyNotFoundException($"Repository '{repositoryName}' was not found.");
}
