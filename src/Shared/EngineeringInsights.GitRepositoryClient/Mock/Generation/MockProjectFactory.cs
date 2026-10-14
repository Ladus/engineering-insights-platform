namespace EngineeringInsights.GitRepositoryClient.Mock.Generation;

internal static class MockProjectFactory
{
    private static readonly string[] ExternalPackages = ["Newtonsoft.Json", "Serilog", "Polly", "Dapper"];

    /// <summary>Creates a project that references the given packages plus one external package.</summary>
    public static MockProject Create(string name, IEnumerable<string> internalReferences, Random random)
    {
        var references = internalReferences
            .Append(ExternalPackages[random.Next(ExternalPackages.Length)])
            .Distinct()
            .ToList();

        var packageId = random.Next(2) == 0 ? name : null;

        return new MockProject($"src/{name}/{name}.csproj", packageId, references);
    }
}
