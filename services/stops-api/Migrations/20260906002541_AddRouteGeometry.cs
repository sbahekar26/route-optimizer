using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StopsApi.Migrations
{
    /// <inheritdoc />
    public partial class AddRouteGeometry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Geometry",
                table: "OptimizationJobs",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Geometry",
                table: "OptimizationJobs");
        }
    }
}
