namespace EngineeringInsights.GitRepositoryClient.Mock;

internal sealed record MockRepository(string AdoProject, string Name, string? OwnerTeam, IReadOnlyList<MockProject> Projects)
{
    private const string ReadmePath = "README.md";
    private const string GitIgnorePath = ".gitignore";

    public IEnumerable<string> FilePaths => new[] { ReadmePath, GitIgnorePath }.Concat(Projects.Select(p => p.Path));

    public string GetFileContent(string path)
    {
        if (string.Equals(path, ReadmePath, StringComparison.OrdinalIgnoreCase))
        {
            return $"# {Name}{Environment.NewLine}";
        }

        if (string.Equals(path, GitIgnorePath, StringComparison.OrdinalIgnoreCase))
        {
            return $"bin/{Environment.NewLine}obj/{Environment.NewLine}";
        }

        var project = Projects.FirstOrDefault(p => string.Equals(p.Path, path, StringComparison.OrdinalIgnoreCase))
            ?? throw new FileNotFoundException($"'{path}' was not found in repository '{Name}'.");

        return project.ToCsproj();
    }
}
