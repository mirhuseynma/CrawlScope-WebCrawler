using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrawlScope.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCrawlSchedules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CrawlSchedules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TargetUrl = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false),
                    MaxDepth = table.Column<int>(type: "int", nullable: false),
                    MaxPages = table.Column<int>(type: "int", nullable: false),
                    StayWithinDomain = table.Column<bool>(type: "bit", nullable: false),
                    IntervalMinutes = table.Column<int>(type: "int", nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NextRunAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastRunAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastCrawlJobId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CrawlSchedules", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CrawlSchedules_IsEnabled",
                table: "CrawlSchedules",
                column: "IsEnabled");

            migrationBuilder.CreateIndex(
                name: "IX_CrawlSchedules_NextRunAt",
                table: "CrawlSchedules",
                column: "NextRunAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CrawlSchedules");
        }
    }
}
