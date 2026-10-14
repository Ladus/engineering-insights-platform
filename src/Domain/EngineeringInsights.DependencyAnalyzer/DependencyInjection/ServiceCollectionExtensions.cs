using EngineeringInsights.DependencyAnalyzer.Services;
using EngineeringInsights.GitRepositoryClient.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace EngineeringInsights.DependencyAnalyzer.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDomainServices(this IServiceCollection services)
    {
        services.AddScoped<ICsProjParserService, CsProjParserService>();
        services.AddScoped<IImportRepositoryService, ImportRepositoryService>();
        services.AddScoped<IDiagramGeneratorService, MermaidDiagramGeneratorService>();
        services.AddGitRepositoryClient();

        return services;
    }
}
