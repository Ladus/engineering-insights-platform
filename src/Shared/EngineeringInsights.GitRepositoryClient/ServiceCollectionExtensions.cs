using EngineeringInsights.GitRepositoryClient.Mock;
using Microsoft.Extensions.DependencyInjection;

namespace EngineeringInsights.GitRepositoryClient;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGitRepositoryClient(this IServiceCollection services)
    {
        services.AddScoped<IGitRepositoryClient, GitRepositoryMockClient>();

        return services;
    }
}
