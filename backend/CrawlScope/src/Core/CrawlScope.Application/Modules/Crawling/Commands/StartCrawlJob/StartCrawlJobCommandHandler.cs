using CrawlScope.Application.Abstractions.Crawling.Services;
using CrawlScope.Application.Abstractions.Persistence;
using CrawlScope.Domain.Modules.Crawling.Enums;
using CrawlScope.Domain.Modules.Crawling.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CrawlScope.Application.Modules.Crawling.Commands.StartCrawlJob
{
    public class StartCrawlJobCommandHandler(
        IAppDbContext context,
        ICrawlQueueProcessor crawlQueueProcessor) : IRequestHandler<StartCrawlJobCommand>
    {
        public async Task Handle(StartCrawlJobCommand request, CancellationToken cancellationToken)
        {
            var crawlJob = await context.CrawlJobs.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (crawlJob is null)
            {
                throw new InvalidOperationException($"Crawl job with ID {request.Id} not found.");
            }

            if (crawlJob.Status != CrawlJobStatus.Pending)
            {
                throw new InvalidOperationException($"Crawl job with ID {request.Id} is not in a pending state.");
            }

            crawlJob.Status = CrawlJobStatus.InProgress;
            crawlJob.StartedAt = DateTime.UtcNow;
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

            await crawlQueueProcessor.ProcessAsync(crawlJob.Id, cancellationToken);
        }
    }
}
