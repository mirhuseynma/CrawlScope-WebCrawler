using CrawlScope.Application.Abstractions.Crawling.Services;
using CrawlScope.Domain.Modules.Crawling.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace CrawlScope.Infrastructure.Crawling.Services
{
    public class PageFetcherFactory(IServiceProvider serviceProvider) : IPageFetcherFactory
    {
        public IPageFetcher Create(CrawlType type)
        {
            return type switch
            {
                CrawlType.Dynamic => serviceProvider.GetRequiredService<PlaywrightPageFetcher>(),
                _ => serviceProvider.GetRequiredService<StandardPageFetcher>()
            };
        }
    }
}
