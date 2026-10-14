using System.Xml.Linq;
using EngineeringInsights.DependencyAnalyzer.Models;

namespace EngineeringInsights.DependencyAnalyzer.Services;

public sealed class CsProjParserService : ICsProjParserService
{
    public ParsedProject Parse(string path, string content)
    {
        var project = XDocument.Parse(content);

        var packageId = project.Descendants("PackageId")
            .Select(e => e.Value.Trim())
            .FirstOrDefault(v => v.Length > 0)
            ?? Path.GetFileNameWithoutExtension(path);

        var packageReferences = project.Descendants("PackageReference")
            .Select(e => e.Attribute("Include")?.Value.Trim())
            .OfType<string>()
            .Where(v => v.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        return new ParsedProject(packageId, packageReferences);
    }
}
