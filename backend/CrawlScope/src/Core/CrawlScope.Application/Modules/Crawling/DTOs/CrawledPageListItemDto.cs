namespace CrawlScope.Application.Modules.Crawling.DTOs
{
    public class CrawledPageListItemDto
    {
        public Guid Id { get; set; }
        public string Url { get; set; } = null!;
        public string? Title { get; set; }
        public string? ContentPreview { get; set; }
        public int? StatusCode { get; set; }
        public int DepthLevel { get; set; }
        public DateTime CrawledAt { get; set; }
        public long? ResponseTimeMs { get; set; }
        public int InternalLinksCount { get; set; }
        public int ExternalLinksCount { get; set; }
    }
}
