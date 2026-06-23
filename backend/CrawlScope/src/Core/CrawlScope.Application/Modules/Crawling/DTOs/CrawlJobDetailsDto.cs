using System;
using System.Collections.Generic;
using System.Text;

namespace CrawlScope.Application.Modules.Crawling.DTOs
{
    public class CrawlJobDetailsDto
    {
        public Guid Id { get; set; }
        public string TargetUrl { get; set; } = null!;
        public string Status { get; set; } = null!;
        public int MaxDepth { get; set; }
        public int MaxPages { get; set; }
        public int PagesFound { get; set; }
        public int PagesFailed { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? FinishedAt { get; set; }
        public string? ErrorMessage { get; set; }

    }
}
