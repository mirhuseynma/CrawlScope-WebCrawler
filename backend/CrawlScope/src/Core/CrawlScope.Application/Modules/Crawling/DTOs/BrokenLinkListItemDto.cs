namespace CrawlScope.Application.Modules.Crawling.DTOs
{
    public class BrokenLinkListItemDto
    {
        public Guid Id { get; set; }
        public Guid CrawlJobId { get; set; }
        public string SourceUrl { get; set; } = null!;
        public string TargetUrl { get; set; } = null!;
        public string? AnchorText { get; set; }
        public bool IsExternal { get; set; }
        public int DepthLevel { get; set; }
        public int? StatusCode { get; set; }
        public long? ResponseTimeMs { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime DetectedAt { get; set; }
    }
}
