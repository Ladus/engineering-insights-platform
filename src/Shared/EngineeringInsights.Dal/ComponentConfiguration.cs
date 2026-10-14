using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EngineeringInsights.Dal;

[Table("ComponentConfigurations")]
public class ComponentConfiguration
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string AdoProjectName { get; set; } = string.Empty;

    [Required]
    public string SourceRepositoryName { get; set; } = string.Empty;

    public string? OwnerTeam { get; set; }

    [Required]
    public string PublishedPackageId { get; set; } = string.Empty;
}