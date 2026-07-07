using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrawlScope.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBrokenLinkReporting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AnchorText",
                table: "CrawlQueueItems",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsExternal",
                table: "CrawlQueueItems",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "ResponseTimeMs",
                table: "CrawlQueueItems",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StatusCode",
                table: "CrawlQueueItems",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AnchorText",
                table: "CrawlQueueItems");

            migrationBuilder.DropColumn(
                name: "IsExternal",
                table: "CrawlQueueItems");

            migrationBuilder.DropColumn(
                name: "ResponseTimeMs",
                table: "CrawlQueueItems");

            migrationBuilder.DropColumn(
                name: "StatusCode",
                table: "CrawlQueueItems");
        }
    }
}
