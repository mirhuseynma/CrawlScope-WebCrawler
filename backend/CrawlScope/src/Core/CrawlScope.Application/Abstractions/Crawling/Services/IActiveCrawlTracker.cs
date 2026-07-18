namespace CrawlScope.Application.Abstractions.Crawling.Services
{
    public interface IActiveCrawlTracker
    {
        void Register(Guid crawlJobId, CancellationTokenSource cts);
        void Unregister(Guid crawlJobId);
        bool Cancel(Guid crawlJobId);
    }
}
