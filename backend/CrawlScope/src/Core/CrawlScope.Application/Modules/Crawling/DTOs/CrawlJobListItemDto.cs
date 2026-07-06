using MediatR.NotificationPublishers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrawlScope.Application.Modules.Crawling.DTOs
{
    public class CrawlJobListItemDto
    {
        public Guid Id { get; set; }
        public string TargetUrl { get; set; } = null!;
        public string Status { get; set; } = null!;
        public int MaxDepth { get; set; }
        public int MaxPages { get; set; }
        public int PagesFound { get; set; }
        public int PagesCrawled { get; set; }
        public int PagesFailed { get; set; }
        public bool IsImportant { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
