using CrawlScope.Domain.Modules.Crawling.Enums;

namespace CrawlScope.Application.Abstractions.Crawling.Services
{
    public interface IPageFetcherFactory
    {
        IPageFetcher Create(CrawlType type);
    }
}
