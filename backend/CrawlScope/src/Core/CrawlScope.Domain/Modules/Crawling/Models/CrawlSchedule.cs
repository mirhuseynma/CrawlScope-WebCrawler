using CrawlScope.Domain.Modules.Crawling.Enums;

namespace CrawlScope.Domain.Modules.Crawling.Models
{
    public class CrawlSchedule : BaseEntity
    {
        public string TargetUrl { get; set; } = null!;
        public int MaxDepth { get; set; }
        public int MaxPages { get; set; }
        public bool StayWithinDomain { get; set; }
        public CrawlType Type { get; set; } = CrawlType.Fast;
        public int IntervalMinutes { get; set; }
        public bool IsEnabled { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime NextRunAt { get; set; }
        public DateTime? LastRunAt { get; set; }
        public Guid? LastCrawlJobId { get; set; }
        public string CreatedBy { get; set; } = null!;
    }
}
