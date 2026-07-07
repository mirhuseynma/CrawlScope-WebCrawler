namespace CrawlScope.Application.Modules.Admin.DTOs
{
    public class AdminOverviewDto
    {
        public AdminOverviewTotalsDto Totals { get; set; } = new();
        public IReadOnlyList<AdminStatusCountDto> StatusDistribution { get; set; } = [];
        public IReadOnlyList<AdminOverviewJobDto> RecentJobs { get; set; } = [];
        public IReadOnlyList<AdminOverviewExportDto> RecentExports { get; set; } = [];
        public IReadOnlyList<AdminOverviewJobDto> ProblemJobs { get; set; } = [];
    }
}
