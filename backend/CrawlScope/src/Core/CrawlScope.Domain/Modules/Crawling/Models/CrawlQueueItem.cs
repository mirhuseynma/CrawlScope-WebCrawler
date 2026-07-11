namespace CrawlScope.Domain.Modules.Crawling.Models
{
    public class CrawlQueueItem : BaseEntity
    {
        public Guid CrawlJobId { get; set; }
        public CrawlJob CrawlJob { get; set; } = null!;
        public string Url { get; set; } = null!;
        public int DepthLevel { get; set; }
        public CrawlQueueStatus Status { get; set; } = CrawlQueueStatus.Pending;
        public int AttemptCount { get; set; } 
        public string? DiscoveredFromUrl { get; set; }
        public string? AnchorText { get; set; }
        public bool IsExternal { get; set; }
        public int? StatusCode { get; set; }
        public long? ResponseTimeMs { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ProcessedAt { get; set; }
        public string? ErrorMessage { get; set; }

    }
}
