
namespace CrawlScope.Application.Modules.Export.DTOs
{
    public class ExportPageRow
    {
        public string Url { get; set; } = null!;
        public string? Title { get; set; }
        public string? Content { get; set; }
        public int? StatusCode { get; set; }
        public int DepthLevel { get; set; }
        public DateTime CrawledAt { get; set; }
        public long? ResponseTimeMs { get; set; }
        public IReadOnlyCollection<ExportLinkRow> Links { get; set; } = [];
    }
}
