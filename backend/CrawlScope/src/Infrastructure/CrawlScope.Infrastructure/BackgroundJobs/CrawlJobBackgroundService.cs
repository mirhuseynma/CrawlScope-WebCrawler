
namespace CrawlScope.Infrastructure.BackgroundJobs
{
    public class CrawlJobBackgroundService : BackgroundService
    {
        private readonly ICrawlJobChannel _channel;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<CrawlJobBackgroundService> _logger;

        public CrawlJobBackgroundService(
            ICrawlJobChannel channel,
            IServiceScopeFactory scopeFactory,
            ILogger<CrawlJobBackgroundService> logger)
        {
            _channel = channel;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("CrawlJob Background Service is starting.");

            try
            {
                await foreach (var crawlJobId in _channel.ReadAllAsync(stoppingToken))
                {
                    _logger.LogInformation("Processing crawl job {CrawlJobId}", crawlJobId);

                    try
                    {
                        using var scope = _scopeFactory.CreateScope();
                        var processor = scope.ServiceProvider.GetRequiredService<ICrawlQueueProcessor>();

                        await processor.ProcessAsync(crawlJobId, stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error occurred executing crawl job {CrawlJobId}.", crawlJobId);
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
