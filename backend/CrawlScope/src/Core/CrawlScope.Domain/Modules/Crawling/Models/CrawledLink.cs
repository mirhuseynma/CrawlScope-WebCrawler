namespace CrawlScope.Domain.Modules.Crawling.Models
{
    public class CrawledLink : BaseEntity
    {
        public Guid CrawledPageId { get; set; }
        public CrawledPage CrawledPage { get; set; } = null!;
        public string SourceUrl { get; set; } = null!;
        public string TargetUrl { get; set; } = null!;
        public string? AnchorText { get; set; }
        public bool IsExternal { get; set; }
        public int DepthLevel { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
