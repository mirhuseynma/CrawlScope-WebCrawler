namespace CrawlScope.Application.Abstractions.Crawling.Services
{
    public interface ICrawlScheduleRunner
    {
        Task RunDueSchedulesAsync(CancellationToken cancellationToken = default);
    }
}
