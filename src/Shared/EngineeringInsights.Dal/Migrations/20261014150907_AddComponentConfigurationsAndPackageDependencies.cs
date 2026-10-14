using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EngineeringInsights.Dal.Migrations
{
    /// <inheritdoc />
    public partial class AddComponentConfigurationsAndPackageDependencies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ComponentConfigurations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AdoProjectName = table.Column<string>(type: "TEXT", nullable: false),
                    SourceRepositoryName = table.Column<string>(type: "TEXT", nullable: false),
                    OwnerTeam = table.Column<string>(type: "TEXT", nullable: true),
                    PublishedPackageId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComponentConfigurations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PackageDependencies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DependentPackageId = table.Column<string>(type: "TEXT", nullable: false),
                    TargetPackageId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackageDependencies", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ComponentConfigurations");

            migrationBuilder.DropTable(
                name: "PackageDependencies");
        }
    }
}
