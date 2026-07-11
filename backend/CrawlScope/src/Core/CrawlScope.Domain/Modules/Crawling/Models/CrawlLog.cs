namespace CrawlScope.Domain.Modules.Crawling.Models
{
    public class CrawlLog : BaseEntity
    {
        public Guid CrawlJobId { get; set; }
        public CrawlJob CrawlJob { get; set; } = null!;
        public CrawlLogLevel Level { get; set; }
        public string Message { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
