namespace CrawlScope.Application.Abstractions.Crawling.Services
{
    public interface ICrawlQueueProcessor
    {
        Task ProcessAsync(
            Guid crawlJobId,
            CancellationToken cancellationToken = default,
            CancellationToken userCancellationToken = default,
            CancellationToken timeoutCancellationToken = default);
    }
}
