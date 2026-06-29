namespace CrawlScope.Application.Modules.Crawling.DTOs
{
    public class CrawlScheduleListItemDto
    {
        public Guid Id { get; set; }
        public string TargetUrl { get; set; } = null!;
        public int MaxDepth { get; set; }
        public int MaxPages { get; set; }
        public bool StayWithinDomain { get; set; }
        public int IntervalMinutes { get; set; }
        public bool IsEnabled { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime NextRunAt { get; set; }
        public DateTime? LastRunAt { get; set; }
        public Guid? LastCrawlJobId { get; set; }
    }
}
