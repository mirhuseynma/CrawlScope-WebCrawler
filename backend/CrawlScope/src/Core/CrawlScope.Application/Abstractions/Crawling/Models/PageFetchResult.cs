namespace CrawlScope.Application.Abstractions.Crawling.Models
{
    public record PageFetchResult(
        string Url,
        int? StatusCode,
        string? Content,
        string? ContentType,
        long ResponseTimeMs,
        bool IsSuccess,
        string? ErrorMessage);
}
