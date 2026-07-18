
namespace CrawlScope.Application.Abstractions.Crawling.Services
{
    public interface IPageFetcherFactory
    {
        IPageFetcher Create(CrawlType type);
    }
}
