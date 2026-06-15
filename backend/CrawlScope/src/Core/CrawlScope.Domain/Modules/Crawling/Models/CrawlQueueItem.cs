using CrawlScope.Domain.Common;
using CrawlScope.Domain.Modules.Crawling.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ProcessedAt { get; set; }
        public string? ErrorMessage { get; set; }

    }
}
