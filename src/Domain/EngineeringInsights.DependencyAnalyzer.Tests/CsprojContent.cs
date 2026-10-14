namespace EngineeringInsights.DependencyAnalyzer.Tests;

internal static class CsprojContent
{
    public static string Create(string? packageId, params string[] packageReferences)
    {
        var packageIdElement = packageId is null ? string.Empty : $"<PackageId>{packageId}</PackageId>";
        var references = string.Concat(packageReferences.Select(r => $"<PackageReference Include=\"{r}\" Version=\"1.0.0\" />"));

        return $"<Project Sdk=\"Microsoft.NET.Sdk\"><PropertyGroup>{packageIdElement}</PropertyGroup><ItemGroup>{references}</ItemGroup></Project>";
    }
}
