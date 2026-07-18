using System.Collections.Concurrent;

namespace CrawlScope.Application.Modules.Crawling.Services
{
    public class ActiveCrawlTracker : IActiveCrawlTracker
    {
        private readonly ConcurrentDictionary<Guid, CancellationTokenSource> _activeCrawls = new();

        public void Register(Guid crawlJobId, CancellationTokenSource cts)
            => _activeCrawls[crawlJobId] = cts;

        public void Unregister(Guid crawlJobId)
            => _activeCrawls.TryRemove(crawlJobId, out _);

        public bool Cancel(Guid crawlJobId)
        {
            if (_activeCrawls.TryGetValue(crawlJobId, out var cts))
            {
                try
                {
                    cts.Cancel();
                    return true;
                }
                catch (ObjectDisposedException)
                {
                    // CTS already disposed, crawl just finished
                }
            }
            return false;
        }
    }
}
