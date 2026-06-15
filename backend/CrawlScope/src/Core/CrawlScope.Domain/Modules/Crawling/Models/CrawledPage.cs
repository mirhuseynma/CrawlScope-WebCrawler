using CrawlScope.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrawlScope.Domain.Modules.Crawling.Models
{
    public class CrawledPage : BaseEntity
    {
        public Guid CrawlJobId { get; set; }
        public CrawlJob CrawlJob { get; set; } = null!;
        public string Url { get; set; } = null!;
        public string? Title { get; set; }
        public string? Content { get; set; }
        public int? StatusCode { get; set; }
        public int DepthLevel { get; set; }
        public DateTime CrawledAt { get; set; }
        public long? ResponseTimeMs { get; set; }
        public ICollection<CrawledLink> Links { get; set; } = [];
    
    }
}
