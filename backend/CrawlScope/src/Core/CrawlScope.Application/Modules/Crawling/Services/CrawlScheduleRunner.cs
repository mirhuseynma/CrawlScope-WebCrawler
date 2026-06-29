using CrawlScope.Application.Abstractions.Crawling.Services;
using CrawlScope.Application.Abstractions.Persistence;
using CrawlScope.Domain.Modules.Crawling.Enums;
using CrawlScope.Domain.Modules.Crawling.Models;
using Microsoft.EntityFrameworkCore;

namespace CrawlScope.Application.Modules.Crawling.Services
{
    public class CrawlScheduleRunner(
        IAppDbContext context,
        ICrawlQueueProcessor crawlQueueProcessor) : ICrawlScheduleRunner
    {
        public async Task RunDueSchedulesAsync(CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            var dueSchedules = await context.CrawlSchedules
                .Where(x => x.IsEnabled && x.NextRunAt <= now)
                .OrderBy(x => x.NextRunAt)
                .Take(5)
                .ToListAsync(cancellationToken);

            foreach (var schedule in dueSchedules)
            {
                var crawlJob = new CrawlJob
                {
                    Id = Guid.NewGuid(),
                    TargetUrl = schedule.TargetUrl,
                    MaxDepth = schedule.MaxDepth,
                    MaxPages = schedule.MaxPages,
                    StayWithinDomain = schedule.StayWithinDomain,
                    Status = CrawlJobStatus.InProgress,
                    CreatedAt = DateTime.UtcNow,
                    StartedAt = DateTime.UtcNow,
                    CreatedBy = schedule.CreatedBy,
                    PagesFound = 1
                };

                await context.CrawlJobs.AddAsync(crawlJob, cancellationToken);
                await context.CrawlQueueItems.AddAsync(new CrawlQueueItem
                {
                    Id = Guid.NewGuid(),
                    CrawlJobId = crawlJob.Id,
                    Url = crawlJob.TargetUrl,
                    DepthLevel = 0,
                    Status = CrawlQueueStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                }, cancellationToken);
                await context.CrawlLogs.AddAsync(new CrawlLog
                {
                    Id = Guid.NewGuid(),
                    CrawlJobId = crawlJob.Id,
                    Level = CrawlLogLevel.Info,
                    Message = $"Scheduled crawl job started for {crawlJob.TargetUrl}.",
                    CreatedAt = DateTime.UtcNow
                }, cancellationToken);

                schedule.LastRunAt = DateTime.UtcNow;
                schedule.LastCrawlJobId = crawlJob.Id;
                schedule.NextRunAt = DateTime.UtcNow.AddMinutes(schedule.IntervalMinutes);

                await context.SaveChangesAsync(cancellationToken);
                await crawlQueueProcessor.ProcessAsync(crawlJob.Id, cancellationToken);
            }
        }
    }
}
