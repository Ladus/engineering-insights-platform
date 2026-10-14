using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EngineeringInsights.Dal;

[Table("PackageDependencies")]
public class PackageDependency
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string DependentPackageId { get; set; } = string.Empty;

    [Required]
    public string TargetPackageId { get; set; } = string.Empty;
}