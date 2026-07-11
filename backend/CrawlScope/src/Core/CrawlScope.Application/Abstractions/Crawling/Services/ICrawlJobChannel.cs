using System;
using System.Threading;
using System.Threading.Tasks;

namespace CrawlScope.Application.Abstractions.Crawling.Services
{
    public interface ICrawlJobChannel
    {
        Task AddCrawlJobAsync(Guid crawlJobId, CancellationToken cancellationToken = default);
        IAsyncEnumerable<Guid> ReadAllAsync(CancellationToken cancellationToken = default);
    }
}
