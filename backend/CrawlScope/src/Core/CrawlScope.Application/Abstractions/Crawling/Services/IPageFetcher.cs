
namespace CrawlScope.Application.Abstractions.Crawling.Services
{
    public interface IPageFetcher
    {
        Task<PageFetchResult> FetchAsync(string url, CancellationToken cancellationToken = default);
    }
}
