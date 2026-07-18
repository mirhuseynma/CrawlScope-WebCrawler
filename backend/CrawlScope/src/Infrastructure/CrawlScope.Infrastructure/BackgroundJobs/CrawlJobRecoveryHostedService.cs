
namespace CrawlScope.Infrastructure.BackgroundJobs
{
    public class CrawlJobRecoveryHostedService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<CrawlJobRecoveryHostedService> _logger;

        public CrawlJobRecoveryHostedService(
            IServiceScopeFactory scopeFactory,
            ILogger<CrawlJobRecoveryHostedService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("CrawlJob Recovery Service is starting.");

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<IAppDbContext>();
                var channel = scope.ServiceProvider.GetRequiredService<ICrawlJobChannel>();

                // Find stuck InProgress jobs and reset to Queued
                var stuckJobs = await context.CrawlJobs
                    .Where(j => j.Status == CrawlJobStatus.InProgress)
                    .ToListAsync(stoppingToken);

                if (stuckJobs.Count > 0)
                {
                    _logger.LogInformation("Found {Count} stuck InProgress jobs. Resetting to Queued.", stuckJobs.Count);
                    foreach (var job in stuckJobs)
                    {
                        job.Status = CrawlJobStatus.Queued;
                    }
                    await context.SaveChangesAsync(stoppingToken);
                }

                // Find all Queued jobs and re-enqueue
                var queuedJobs = await context.CrawlJobs
                    .Where(j => j.Status == CrawlJobStatus.Queued)
                    .ToListAsync(stoppingToken);

                if (queuedJobs.Count > 0)
                {
                    _logger.LogInformation("Found {Count} Queued jobs. Re-enqueuing.", queuedJobs.Count);
                    foreach (var job in queuedJobs)
                    {
                        await channel.AddCrawlJobAsync(job.Id, stoppingToken);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during CrawlJob recovery.");
            }

            _logger.LogInformation("CrawlJob Recovery Service has finished.");
        }
    }
}
