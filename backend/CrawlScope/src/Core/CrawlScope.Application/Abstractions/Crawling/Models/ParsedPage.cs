namespace CrawlScope.Application.Abstractions.Crawling.Models
{
    public record ParsedPage(
        string SourceUrl,
        string? Title,
        string? TextContent,
        IReadOnlyCollection<ParsedLink> Links);
}
