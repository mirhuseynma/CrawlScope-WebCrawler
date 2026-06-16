using CrawlScope.Domain.Common;
using CrawlScope.Domain.Modules.Crawling.Enums;
using CrawlScope.Domain.Modules.Export.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
