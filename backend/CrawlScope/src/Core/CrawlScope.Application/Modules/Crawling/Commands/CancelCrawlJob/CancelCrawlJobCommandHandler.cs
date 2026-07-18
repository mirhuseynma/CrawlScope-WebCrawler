using CrawlScope.Domain.Modules.Crawling.Models;

namespace CrawlScope.Application.Modules.Crawling.Commands.CancelCrawlJob
{
    public class CancelCrawlJobCommandHandler(
        IAppDbContext context,
        IActiveCrawlTracker activeCrawlTracker) : IRequestHandler<CancelCrawlJobCommand>
    {
        public async Task Handle(CancelCrawlJobCommand request, CancellationToken cancellationToken)
        {
            var crawlJob = await context.CrawlJobs
                .FirstOrDefaultAsync(
                    x => x.Id == request.Id
                        && (request.IncludeAllUsers || x.CreatedBy == request.RequestingUserId),
                    cancellationToken)
                ?? throw new NotFoundException($"Crawl job with ID {request.Id} not found.");

            if (crawlJob.Status != CrawlJobStatus.InProgress && crawlJob.Status != CrawlJobStatus.Queued)
            {
                throw new InvalidOperationException(
                    $"Crawl job cannot be canceled in its current state: {crawlJob.Status}.");
            }

            // For InProgress jobs: signal the running processor to stop via its CTS.
            // ProcessAsync will catch the OperationCanceledException and update the DB itself.
            bool signaledInMemory = activeCrawlTracker.Cancel(crawlJob.Id);

            if (!signaledInMemory)
            {
                // Job is Queued (waiting in channel) or just not yet registered —
                // mark it directly in the DB. ProcessAsync will see Canceled status and return early.
                crawlJob.Status = CrawlJobStatus.Canceled;
                crawlJob.FinishedAt = DateTime.UtcNow;

                await context.CrawlLogs.AddAsync(new CrawlLog
                {
                    Id = Guid.NewGuid(),
                    CrawlJobId = crawlJob.Id,
                    Level = CrawlLogLevel.Warning,
                    Message = "Crawl job was canceled by user before it started processing.",
                    CreatedAt = DateTime.UtcNow
                }, cancellationToken);

                await context.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
