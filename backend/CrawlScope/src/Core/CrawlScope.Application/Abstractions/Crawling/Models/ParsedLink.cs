namespace CrawlScope.Application.Abstractions.Crawling.Models
{
    public record ParsedLink(
        string SourceUrl,
        string TargetUrl,
        string? AnchorText,
        bool IsExternal);
}
