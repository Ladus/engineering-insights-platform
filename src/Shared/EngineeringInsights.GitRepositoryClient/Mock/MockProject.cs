namespace EngineeringInsights.GitRepositoryClient.Mock;

internal sealed record MockProject(string Path, string? PackageId, IReadOnlyList<string> PackageReferences)
{
    public string ToCsproj()
    {
        var packageIdElement = PackageId is null
            ? string.Empty
            : $"    <PackageId>{PackageId}</PackageId>{Environment.NewLine}";

        var packageReferences = string.Join(
            Environment.NewLine,
            PackageReferences.Select(r => $"    <PackageReference Include=\"{r}\" Version=\"1.0.0\" />"));

        return $"""
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>net10.0</TargetFramework>
            {packageIdElement}  </PropertyGroup>
              <ItemGroup>
            {packageReferences}
              </ItemGroup>
            </Project>
            """;
    }
}
