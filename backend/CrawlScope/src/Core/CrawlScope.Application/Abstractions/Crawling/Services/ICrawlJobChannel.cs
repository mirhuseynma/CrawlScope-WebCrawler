
namespace CrawlScope.Application.Abstractions.Crawling.Services
{
    public interface ICrawlJobChannel
    {
        Task AddCrawlJobAsync(Guid crawlJobId, CancellationToken cancellationToken = default);
        IAsyncEnumerable<Guid> ReadAllAsync(CancellationToken cancellationToken = default);
    }
}
