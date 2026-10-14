namespace EngineeringInsights.GitRepositoryClient.Mock.Generation;

internal static class MockRepositoryCatalog
{
    private const string PlatformTeam = "Engineering Platform";

    private static readonly (string Repository, string[] Packages)[] PlatformRepositories =
    [
        ("platform-foundation", ["RB.Platform.Core", "RB.Platform.Configuration"]),
        ("platform-observability", ["RB.Platform.Logging", "RB.Platform.Telemetry"]),
        ("platform-messaging", ["RB.Platform.Messaging", "RB.Platform.Security"])
    ];

    private static readonly string[] Products = ["Payments", "Lending", "Savings"];
    private static readonly string[] SharedPackageNames = ["Contracts", "Client"];

    private static readonly (string Repository, string[] Applications)[] ApplicationRepositories =
    [
        ("api", ["Api"]),
        ("app", ["App", "Worker"])
    ];

    /// <summary>Builds the same repositories on every call because of the fixed seed.</summary>
    public static IReadOnlyList<MockRepository> Create()
    {
        var random = new Random(1);

        var (platformRepositories, platformPackages) = CreatePlatformTier(random);
        var (sharedRepositories, sharedPackagesByProduct) = CreateSharedPackageTier(random, platformPackages);
        var applicationRepositories = CreateApplicationTier(random, platformPackages, sharedPackagesByProduct);

        return [.. platformRepositories, .. sharedRepositories, .. applicationRepositories];
    }

    // Tier 1: platform packages built for, and used by, all other teams.
    private static (List<MockRepository> Repositories, List<string> Packages) CreatePlatformTier(Random random)
    {
        var repositories = new List<MockRepository>();
        var platformPackages = new List<string>();

        foreach (var (repository, packages) in PlatformRepositories)
        {
            var projects = new List<MockProject>();
            foreach (var package in packages)
            {
                // Every platform package builds on the first one, so the generated graph has no cycles.
                projects.Add(MockProjectFactory.Create(package, platformPackages.Take(1), random));
                platformPackages.Add(package);
            }

            repositories.Add(new MockRepository("Platform", repository, PlatformTeam, projects));
        }

        return (repositories, platformPackages);
    }

    // Tier 2: shared packages of a product team, used by its own applications and by other teams.
    private static (List<MockRepository> Repositories, Dictionary<string, List<string>> PackagesByProduct) CreateSharedPackageTier(
        Random random,
        IReadOnlyList<string> platformPackages)
    {
        var repositories = new List<MockRepository>();
        var packagesByProduct = new Dictionary<string, List<string>>();

        foreach (var product in Products)
        {
            // Only shared packages of earlier products are referenced, so the generated graph has no cycles.
            var otherTeamPackages = packagesByProduct.Values.SelectMany(p => p).ToList();
            var ownPackages = new List<string>();
            var projects = new List<MockProject>();

            foreach (var packageName in SharedPackageNames)
            {
                var name = $"RB.{product}.{packageName}";
                var references = MockRandom.Pick(random, platformPackages, 1, 3)
                    .Concat(ownPackages)
                    .Concat(MockRandom.Pick(random, otherTeamPackages, 0, 2));

                projects.Add(MockProjectFactory.Create(name, references, random));
                ownPackages.Add(name);
            }

            packagesByProduct[product] = ownPackages;
            repositories.Add(new MockRepository(product, $"{product.ToLowerInvariant()}-shared", MockRandom.OwnerTeam(random, product), projects));
        }

        return (repositories, packagesByProduct);
    }

    // Tier 3: applications of a product team, which nothing references.
    private static List<MockRepository> CreateApplicationTier(
        Random random,
        IReadOnlyList<string> platformPackages,
        IReadOnlyDictionary<string, List<string>> sharedPackagesByProduct)
    {
        var repositories = new List<MockRepository>();

        foreach (var (product, ownPackages) in sharedPackagesByProduct)
        {
            var otherTeamPackages = sharedPackagesByProduct
                .Where(p => p.Key != product)
                .SelectMany(p => p.Value)
                .ToList();

            foreach (var (repository, applications) in ApplicationRepositories)
            {
                var projects = applications
                    .Select(application => MockProjectFactory.Create(
                        $"RB.{product}.{application}",
                        MockRandom.Pick(random, platformPackages, 1, 3)
                            .Concat(ownPackages)
                            .Concat(MockRandom.Pick(random, otherTeamPackages, 0, 2)),
                        random))
                    .ToList();

                repositories.Add(new MockRepository(product, $"{product.ToLowerInvariant()}-{repository}", MockRandom.OwnerTeam(random, product), projects));
            }
        }

        return repositories;
    }
}
