using CrawlScope.Domain.Modules.Export.Models;

namespace CrawlScope.Domain.Modules.Crawling.Models
{
    public class CrawlJob : BaseEntity
    {
        public string TargetUrl { get; set; } = null!;
        public int MaxDepth { get; set; }
        public int MaxPages { get; set; }
        public bool StayWithinDomain { get; set; }
        public CrawlJobStatus Status { get; set; } = CrawlJobStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? StartedAt { get; set; }
        public DateTime? FinishedAt { get; set; }
        public string CreatedBy { get; set; } = null!;
        public bool IsImportant { get; set; }
        public int PagesFound { get; set; }
        public int PagesCrawled { get; set; }
        public int PagesFailed { get; set; }
        public string? ErrorMessage { get; set; }
        public ICollection<CrawlQueueItem> QueueItems { get; set; } = [];
        public ICollection<CrawledPage> CrawledPages { get; set; } = [];
        public ICollection<CrawlLog> Logs { get; set;} = [];
        public ICollection<ExportFile> ExportFiles { get; set; } = [];
    }
}
