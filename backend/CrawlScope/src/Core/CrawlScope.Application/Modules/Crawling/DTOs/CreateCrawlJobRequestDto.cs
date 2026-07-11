namespace CrawlScope.Application.Modules.Crawling.DTOs
{
    public class CreateCrawlJobRequestDto
    {
        public string TargetUrl { get; set; } = null!;
        public int MaxDepth { get; set; }
        public bool StayWithinDomain { get; set; }
        public int MaxPages { get; set; }
    }
}
