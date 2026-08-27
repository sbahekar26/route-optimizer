using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StopsApi.Migrations
{
    /// <inheritdoc />
    public partial class AddOptimizationJob : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OptimizationJobs",
                columns: table => new
                {
                    JobId = table.Column<Guid>(type: "uuid", nullable: false),
                    Route = table.Column<long[]>(type: "bigint[]", nullable: false),
                    TotalCost = table.Column<long>(type: "bigint", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OptimizationJobs", x => x.JobId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OptimizationJobs");
        }
    }
}
