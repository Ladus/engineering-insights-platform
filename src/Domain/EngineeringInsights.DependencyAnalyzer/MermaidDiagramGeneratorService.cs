using System.Text;
using EngineeringInsights.Dal;
using Microsoft.EntityFrameworkCore;

namespace EngineeringInsights.DependencyAnalyzer;

/// <summary>Generates the dependency diagram as Mermaid <c>graph TD</c> text.</summary>
public sealed class MermaidDiagramGeneratorService(EngineeringInsightsDbContext dbContext) : IDiagramGeneratorService
{
    public async Task<string> GenerateDiagramAsync()
    {
        var (packageIds, edges) = await LoadGraphAsync();

        return RenderDiagram(packageIds, edges);
    }

    public async Task<string> GenerateDiagramAsync(string packageId)
    {
        var (packageIds, edges) = await LoadGraphAsync();

        var root = packageIds.FirstOrDefault(id => string.Equals(id, packageId, StringComparison.OrdinalIgnoreCase))
            ?? throw new KeyNotFoundException($"Package '{packageId}' was not found.");

        var connected = CollectReachable(root, edges, towardsDependencies: true);
        connected.UnionWith(CollectReachable(root, edges, towardsDependencies: false));

        var connectedEdges = edges
            .Where(e => connected.Contains(e.From) && connected.Contains(e.To))
            .ToList();

        return RenderDiagram(connected, connectedEdges);
    }

    // Only packages published by a project in our repositories are part of the graph; external packages are left out.
    private async Task<(List<string> PackageIds, List<(string From, string To)> Edges)> LoadGraphAsync()
    {
        var publishedPackageIds = await dbContext.ComponentConfigurations
            .AsNoTracking()
            .Select(c => c.PublishedPackageId)
            .ToListAsync();

        var packageIds = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var publishedPackageId in publishedPackageIds)
        {
            packageIds.TryAdd(publishedPackageId, publishedPackageId);
        }

        var dependencies = await dbContext.PackageDependencies
            .AsNoTracking()
            .ToListAsync();

        var edges = new List<(string From, string To)>();
        foreach (var dependency in dependencies)
        {
            if (packageIds.TryGetValue(dependency.DependentPackageId, out var from)
                && packageIds.TryGetValue(dependency.TargetPackageId, out var to))
            {
                edges.Add((from, to));
            }
        }

        return (packageIds.Values.ToList(), edges);
    }

    private static string RenderDiagram(IEnumerable<string> packageIds, IReadOnlyList<(string From, string To)> edges)
    {
        var nodeIds = packageIds
            .Distinct(StringComparer.Ordinal)
            .OrderBy(id => id, StringComparer.Ordinal)
            .Select((packageId, index) => (PackageId: packageId, NodeId: $"n{index}"))
            .ToList();

        var nodeIdByPackageId = nodeIds.ToDictionary(n => n.PackageId, n => n.NodeId, StringComparer.Ordinal);

        var diagram = new StringBuilder("graph TD\n");

        foreach (var (packageId, nodeId) in nodeIds)
        {
            diagram.Append($"    {nodeId}[\"{packageId.Replace("\"", "#quot;")}\"]\n");
        }

        // Arrows point from a package to the packages that use it, which places shared packages at the top.
        var edgeNodeIds = edges
            .Select(e => (From: nodeIdByPackageId[e.To], To: nodeIdByPackageId[e.From]))
            .Distinct()
            .OrderBy(e => e.From, StringComparer.Ordinal)
            .ThenBy(e => e.To, StringComparer.Ordinal);

        foreach (var (from, to) in edgeNodeIds)
        {
            diagram.Append($"    {from} --> {to}\n");
        }

        return diagram.ToString();
    }

    private static HashSet<string> CollectReachable(
        string root,
        IReadOnlyList<(string From, string To)> edges,
        bool towardsDependencies)
    {
        var neighbours = edges.ToLookup(
            e => towardsDependencies ? e.From : e.To,
            e => towardsDependencies ? e.To : e.From,
            StringComparer.Ordinal);

        var reached = new HashSet<string>(StringComparer.Ordinal) { root };
        var pending = new Queue<string>([root]);

        while (pending.Count > 0)
        {
            foreach (var next in neighbours[pending.Dequeue()])
            {
                if (reached.Add(next))
                {
                    pending.Enqueue(next);
                }
            }
        }

        return reached;
    }
}
