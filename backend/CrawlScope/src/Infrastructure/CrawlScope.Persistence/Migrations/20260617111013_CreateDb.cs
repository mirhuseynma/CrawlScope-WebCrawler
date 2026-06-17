using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrawlScope.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CreateDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CrawlJobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TargetUrl = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false),
                    MaxDepth = table.Column<int>(type: "int", nullable: false),
                    MaxPages = table.Column<int>(type: "int", nullable: false),
                    StayWithinDomain = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FinishedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PagesFound = table.Column<int>(type: "int", nullable: false),
                    PagesCrawled = table.Column<int>(type: "int", nullable: false),
                    PagesFailed = table.Column<int>(type: "int", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CrawlJobs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CrawledPages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CrawlJobId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StatusCode = table.Column<int>(type: "int", nullable: true),
                    DepthLevel = table.Column<int>(type: "int", nullable: false),
                    CrawledAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ResponseTimeMs = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CrawledPages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CrawledPages_CrawlJobs_CrawlJobId",
                        column: x => x.CrawlJobId,
                        principalTable: "CrawlJobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CrawlLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CrawlJobId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Level = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CrawlLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CrawlLogs_CrawlJobs_CrawlJobId",
                        column: x => x.CrawlJobId,
                        principalTable: "CrawlJobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CrawlQueueItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CrawlJobId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false),
                    DepthLevel = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AttemptCount = table.Column<int>(type: "int", nullable: false),
                    DiscoveredFromUrl = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CrawlQueueItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CrawlQueueItems_CrawlJobs_CrawlJobId",
                        column: x => x.CrawlJobId,
                        principalTable: "CrawlJobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExportFiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CrawlJobId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Format = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExportFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExportFiles_CrawlJobs_CrawlJobId",
                        column: x => x.CrawlJobId,
                        principalTable: "CrawlJobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CrawledLinks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CrawledPageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceUrl = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false),
                    TargetUrl = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false),
                    AnchorText = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    IsExternal = table.Column<bool>(type: "bit", nullable: false),
                    DepthLevel = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CrawledLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CrawledLinks_CrawledPages_CrawledPageId",
                        column: x => x.CrawledPageId,
                        principalTable: "CrawledPages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CrawledLinks_CrawledPageId_TargetUrl",
                table: "CrawledLinks",
                columns: new[] { "CrawledPageId", "TargetUrl" });

            migrationBuilder.CreateIndex(
                name: "IX_CrawledPages_CrawlJobId_Url",
                table: "CrawledPages",
                columns: new[] { "CrawlJobId", "Url" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CrawlLogs_CrawlJobId",
                table: "CrawlLogs",
                column: "CrawlJobId");

            migrationBuilder.CreateIndex(
                name: "IX_CrawlQueueItems_CrawlJobId_Url",
                table: "CrawlQueueItems",
                columns: new[] { "CrawlJobId", "Url" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExportFiles_CrawlJobId",
                table: "ExportFiles",
                column: "CrawlJobId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CrawledLinks");

            migrationBuilder.DropTable(
                name: "CrawlLogs");

            migrationBuilder.DropTable(
                name: "CrawlQueueItems");

            migrationBuilder.DropTable(
                name: "ExportFiles");

            migrationBuilder.DropTable(
                name: "CrawledPages");

            migrationBuilder.DropTable(
                name: "CrawlJobs");
        }
    }
}
