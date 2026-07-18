
namespace CrawlScope.Infrastructure.BackgroundJobs
{
    public class CrawlJobBackgroundService : BackgroundService
    {
        private readonly ICrawlJobChannel _channel;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<CrawlJobBackgroundService> _logger;
        private readonly IActiveCrawlTracker _tracker;

        public CrawlJobBackgroundService(
            ICrawlJobChannel channel,
            IServiceScopeFactory scopeFactory,
            ILogger<CrawlJobBackgroundService> logger,
            IActiveCrawlTracker tracker)
        {
            _channel = channel;
            _scopeFactory = scopeFactory;
            _logger = logger;
            _tracker = tracker;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("CrawlJob Background Service is starting.");

            try
            {
                await foreach (var crawlJobId in _channel.ReadAllAsync(stoppingToken))
                {
                    _logger.LogInformation("Processing crawl job {CrawlJobId}", crawlJobId);

                    // CTS 1: 10-minute hard timeout
                    using var timeoutCts = new CancellationTokenSource(TimeSpan.FromMinutes(10));

                    // CTS 2: user-triggered cancellation (registered in tracker)
                    using var userCts = new CancellationTokenSource();

                    // CTS 3: linked — fires when ANY of the three sources triggers
                    using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
                        stoppingToken, timeoutCts.Token, userCts.Token);

                    _tracker.Register(crawlJobId, userCts);

                    try
                    {
                        using var scope = _scopeFactory.CreateScope();
                        var processor = scope.ServiceProvider.GetRequiredService<ICrawlQueueProcessor>();

                        await processor.ProcessAsync(
                            crawlJobId,
                            linkedCts.Token,
                            userCts.Token,
                            timeoutCts.Token);
                    }
                    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                    {
                        // Service is shutting down — exit the loop cleanly
                        _logger.LogInformation("CrawlJob Background Service stopping mid-job {CrawlJobId}.", crawlJobId);
                        break;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error occurred executing crawl job {CrawlJobId}.", crawlJobId);
                    }
                    finally
                    {
                        _tracker.Unregister(crawlJobId);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Prevent throwing if stoppingToken was canceled
            }

            _logger.LogInformation("CrawlJob Background Service is stopping.");
        }
    }
}
