using EngineeringInsights.Dal;
using EngineeringInsights.DependencyAnalyzer.Models;
using EngineeringInsights.DependencyAnalyzer.Services;
using EngineeringInsights.GitRepositoryClient;
using Microsoft.EntityFrameworkCore;

namespace EngineeringInsights.DependencyAnalyzer;

public sealed class ImportRepositoryService(
    IGitRepositoryClient gitClient,
    ICsProjParserService csProjParser,
    EngineeringInsightsDbContext dbContext)
    : IImportRepositoryService
{
    private const string ProjectFileSuffix = ".csproj";

    public async Task ImportAllRepositoriesAsync()
    {
        var repositoryNames = (await gitClient.GetRepositoriesAsync())
            .Select(r => r.RepoName)
            .ToList();

        var lowerCaseNames = repositoryNames.Select(n => n.ToLower()).ToList();
        var removedComponents = await dbContext.ComponentConfigurations
            .Where(c => !lowerCaseNames.Contains(c.SourceRepositoryName.ToLower()))
            .ToListAsync();

        await RemoveComponentsAsync(removedComponents);
        await dbContext.SaveChangesAsync();

        foreach (var repositoryName in repositoryNames)
        {
            await ImportRepositoryAsync(repositoryName);
        }
    }

    public async Task ImportRepositoryAsync(string repositoryName)
    {
        // Everything is read from the client first so a failure leaves the stored data untouched.
        // The name filter is a "contains", so the exact suffix is still checked here.
        var projectFiles = gitClient.GetFilesAsync(repositoryName, ProjectFileSuffix)
            .Where(f => f.Path.EndsWith(ProjectFileSuffix, StringComparison.OrdinalIgnoreCase));

        var projects = new List<(GitFileDto File, ParsedProject Parsed)>();
        await foreach (var file in projectFiles)
        {
            var content = await gitClient.GetFileContentAsync(repositoryName, file.Path);
            projects.Add((file, csProjParser.Parse(file.Path, content)));
        }

        var ownerTeam = await gitClient.GetOwnerTeamAsync(repositoryName);

        var lowerCaseName = repositoryName.ToLower();
        var existingComponents = await dbContext.ComponentConfigurations
            .Where(c => c.SourceRepositoryName.ToLower() == lowerCaseName)
            .ToListAsync();

        await RemoveComponentsAsync(existingComponents);

        foreach (var (file, parsed) in projects)
        {
            dbContext.ComponentConfigurations.Add(new ComponentConfiguration
            {
                AdoProjectName = file.AdoProjectName,
                SourceRepositoryName = file.RepositoryName,
                OwnerTeam = ownerTeam,
                PublishedPackageId = parsed.PackageId
            });

            foreach (var packageReference in parsed.PackageReferences)
            {
                dbContext.PackageDependencies.Add(new PackageDependency
                {
                    DependentPackageId = parsed.PackageId,
                    TargetPackageId = packageReference
                });
            }
        }

        await dbContext.SaveChangesAsync();
    }

    private async Task RemoveComponentsAsync(List<ComponentConfiguration> components)
    {
        var packageIds = components
            .Select(c => c.PublishedPackageId)
            .Distinct()
            .ToList();

        var dependencies = await dbContext.PackageDependencies
            .Where(d => packageIds.Contains(d.DependentPackageId))
            .ToListAsync();

        dbContext.PackageDependencies.RemoveRange(dependencies);
        dbContext.ComponentConfigurations.RemoveRange(components);
    }
}
