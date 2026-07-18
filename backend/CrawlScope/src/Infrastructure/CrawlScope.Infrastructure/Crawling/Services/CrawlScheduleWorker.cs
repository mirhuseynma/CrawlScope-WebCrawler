
namespace CrawlScope.Infrastructure.Crawling.Services
{
    public class CrawlScheduleWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<CrawlScheduleWorker> logger) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = scopeFactory.CreateScope();
                    var runner = scope.ServiceProvider.GetRequiredService<ICrawlScheduleRunner>();
                    await runner.RunDueSchedulesAsync(cancellationToken);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    return;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to run due crawl schedules.");
                }

                await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken);
            }
        }
    }
}
