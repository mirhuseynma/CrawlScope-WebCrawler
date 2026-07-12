
namespace CrawlScope.Application.Modules.Crawling.Commands.StartCrawlJob
{
    public class StartCrawlJobCommandHandler(
        IAppDbContext context,
        ICrawlJobChannel crawlJobChannel) : IRequestHandler<StartCrawlJobCommand>
    {
        public async Task Handle(StartCrawlJobCommand request, CancellationToken cancellationToken)
        {
            var crawlJob = await context.CrawlJobs
                .FirstOrDefaultAsync(
                    x => x.Id == request.Id
                        && (request.IncludeAllUsers || x.CreatedBy == request.RequestingUserId),
                    cancellationToken)
                ?? throw new NotFoundException($"Crawl job with ID {request.Id} not found.");

            if (crawlJob.Status != CrawlJobStatus.Pending)
            {
                throw new InvalidOperationException($"Crawl job with ID {request.Id} is not in a pending state.");
            }

            crawlJob.Status = CrawlJobStatus.Queued;
            crawlJob.PagesFound = 1;

            var queueItemExists = await context.CrawlQueueItems
                .AnyAsync(x => x.CrawlJobId == crawlJob.Id && x.Url == crawlJob.TargetUrl, cancellationToken);

            if (!queueItemExists)
            {
                await context.CrawlQueueItems.AddAsync(new CrawlQueueItem
                {
                    Id = Guid.NewGuid(),
                    CrawlJobId = crawlJob.Id,
                    Url = crawlJob.TargetUrl,
                    DepthLevel = 0,
                    Status = CrawlQueueStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                }, cancellationToken);
            }

            await context.CrawlLogs.AddAsync(new CrawlLog
            {
                Id = Guid.NewGuid(),
                CrawlJobId = crawlJob.Id,
                Level = CrawlLogLevel.Info,
                Message = $"Crawl job started for {crawlJob.TargetUrl}.",
                CreatedAt = DateTime.UtcNow
            }, cancellationToken);

            await context.SaveChangesAsync(cancellationToken);

            await crawlJobChannel.AddCrawlJobAsync(crawlJob.Id, cancellationToken);
        }
    }
}
