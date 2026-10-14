using EngineeringInsights.DependencyAnalyzer.Models;

namespace EngineeringInsights.DependencyAnalyzer.Services;

public interface ICsProjParserService
{
    /// <summary>
    /// Parses a .csproj file into the package it publishes (defaulting to the file name) and the packages it references.
    /// </summary>
    /// <param name="path">The path of the .csproj file.</param>
    /// <param name="content">The XML content of the .csproj file.</param>
    ParsedProject Parse(string path, string content);
}
