using CrawlScope.Domain.Modules.Crawling.Enums;

namespace CrawlScope.Application.Modules.Crawling.DTOs
{
    public class CreateCrawlScheduleRequestDto
    {
        public string TargetUrl { get; set; } = null!;
        public int MaxDepth { get; set; }
        public int MaxPages { get; set; }
        public bool StayWithinDomain { get; set; }
        public CrawlType CrawlType { get; set; } = CrawlType.Fast;
        public int IntervalMinutes { get; set; }
    }
}
