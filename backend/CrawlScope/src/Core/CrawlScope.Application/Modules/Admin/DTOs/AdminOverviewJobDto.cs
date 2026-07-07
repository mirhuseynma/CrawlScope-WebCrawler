namespace CrawlScope.Application.Modules.Admin.DTOs
{
    public class AdminOverviewJobDto
    {
        public Guid Id { get; set; }
        public string TargetUrl { get; set; } = null!;
        public string Status { get; set; } = null!;
        public int PagesCrawled { get; set; }
        public int PagesFailed { get; set; }
        public bool IsImportant { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
