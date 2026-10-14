namespace EngineeringInsights.DependencyAnalyzer.Models;

public sealed record ParsedProject(string PackageId, IReadOnlyList<string> PackageReferences);
