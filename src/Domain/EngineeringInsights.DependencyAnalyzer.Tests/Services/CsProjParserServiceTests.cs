using EngineeringInsights.DependencyAnalyzer.Services;
using System.Xml;

namespace EngineeringInsights.DependencyAnalyzer.Tests.Services;

public class CsProjParserServiceTests
{
    private readonly CsProjParserService _parser = new();

    [Fact]
    public void Parse_WithPackageId_ReturnsPackageIdAndReferences()
    {
        // Arrange
        var content = CsprojContent.Create("Contoso.Core", "Newtonsoft.Json", "Serilog");

        // Act
        var parsed = _parser.Parse("src/Core/Core.csproj", content);

        // Assert
        Assert.Equal("Contoso.Core", parsed.PackageId);
        Assert.Equal(["Newtonsoft.Json", "Serilog"], parsed.PackageReferences);
    }

    [Fact]
    public void Parse_WithoutPackageId_UsesFileNameWithoutExtension()
    {
        // Arrange
        var content = CsprojContent.Create(null, "Serilog");

        // Act
        var parsed = _parser.Parse("src/Core/Contoso.Core.csproj", content);

        // Assert
        Assert.Equal("Contoso.Core", parsed.PackageId);
    }

    [Fact]
    public void Parse_WithBlankPackageId_UsesFileNameWithoutExtension()
    {
        // Arrange
        var content = "<Project><PropertyGroup><PackageId>   </PackageId></PropertyGroup></Project>";

        // Act
        var parsed = _parser.Parse("src/Core/Core.csproj", content);

        // Assert
        Assert.Equal("Core", parsed.PackageId);
    }

    [Fact]
    public void Parse_TrimsPackageIdAndReferences()
    {
        // Arrange
        var content = """
            <Project>
              <PropertyGroup><PackageId>  Contoso.Core  </PackageId></PropertyGroup>
              <ItemGroup><PackageReference Include="  Serilog  " /></ItemGroup>
            </Project>
            """;

        // Act
        var parsed = _parser.Parse("src/Core/Core.csproj", content);

        // Assert
        Assert.Equal("Contoso.Core", parsed.PackageId);
        Assert.Equal(["Serilog"], parsed.PackageReferences);
    }

    [Fact]
    public void Parse_WithDuplicateReferencesIgnoringCasing_ReturnsEachOnce()
    {
        // Arrange
        var content = CsprojContent.Create("Contoso.Core", "Serilog", "serilog", "Polly");

        // Act
        var parsed = _parser.Parse("src/Core/Core.csproj", content);

        // Assert
        Assert.Equal(["Serilog", "Polly"], parsed.PackageReferences);
    }

    [Fact]
    public void Parse_WithoutReferences_ReturnsEmptyList()
    {
        // Arrange
        var content = CsprojContent.Create("Contoso.Core");

        // Act
        var parsed = _parser.Parse("src/Core/Core.csproj", content);

        // Assert
        Assert.Empty(parsed.PackageReferences);
    }

    [Fact]
    public void Parse_IgnoresReferencesWithoutInclude()
    {
        // Arrange
        var content = """
            <Project>
              <ItemGroup>
                <PackageReference Update="Serilog" />
                <PackageReference Include="" />
                <PackageReference Include="Polly" />
              </ItemGroup>
            </Project>
            """;

        // Act
        var parsed = _parser.Parse("src/Core/Core.csproj", content);

        // Assert
        Assert.Equal(["Polly"], parsed.PackageReferences);
    }

    [Fact]
    public void Parse_WithInvalidXml_Throws()
    {
        // Arrange
        var content = "# not a project file";

        // Act
        var act = () => _parser.Parse("src/Core/Core.csproj", content);

        // Assert
        Assert.Throws<XmlException>(act);
    }
}
