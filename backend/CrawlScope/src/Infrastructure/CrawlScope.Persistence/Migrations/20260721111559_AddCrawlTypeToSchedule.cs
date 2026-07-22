using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrawlScope.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCrawlTypeToSchedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "CrawlSchedules",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "CrawlSchedules");
        }
    }
}
