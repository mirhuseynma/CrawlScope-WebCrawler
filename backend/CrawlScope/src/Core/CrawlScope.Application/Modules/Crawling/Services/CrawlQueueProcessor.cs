using CrawlScope.Application.Abstractions.Crawling.Models;
using CrawlScope.Application.Abstractions.Crawling.Services;
using CrawlScope.Application.Abstractions.Persistence;
using CrawlScope.Application.Common.Exceptions;
using CrawlScope.Domain.Modules.Crawling.Enums;
using CrawlScope.Domain.Modules.Crawling.Models;
using Microsoft.EntityFrameworkCore;

namespace CrawlScope.Application.Modules.Crawling.Services
{
    public class CrawlQueueProcessor(
        IAppDbContext context,
        IPageFetcher pageFetcher,
        IHtmlParser htmlParser) : ICrawlQueueProcessor
    {
        public async Task ProcessAsync(Guid crawlJobId, CancellationToken cancellationToken = default)
        {
            var crawlJob = await context.CrawlJobs
                .FirstOrDefaultAsync(x => x.Id == crawlJobId, cancellationToken);

            if (crawlJob is null)
            {
                throw new NotFoundException($"Crawl job with ID {crawlJobId} not found.");
            }

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var queueItem = await GetNextPendingQueueItemAsync(crawlJobId, cancellationToken);
                    if (queueItem is null || crawlJob.PagesCrawled >= crawlJob.MaxPages)
                    {
                        break;
                    }

                    await ProcessQueueItemAsync(crawlJob, queueItem, cancellationToken);
                    await context.SaveChangesAsync(cancellationToken);
                }

                crawlJob.Status = CrawlJobStatus.Completed;
                crawlJob.FinishedAt = DateTime.UtcNow;

                await AddLogAsync(crawlJob.Id, CrawlLogLevel.Info, "Crawl job completed.", cancellationToken);
                await context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                crawlJob.Status = CrawlJobStatus.Failed;
                crawlJob.FinishedAt = DateTime.UtcNow;
                crawlJob.ErrorMessage = ex.Message;

                await AddLogAsync(crawlJob.Id, CrawlLogLevel.Error, $"Crawl job failed: {ex.Message}", cancellationToken);
                await context.SaveChangesAsync(cancellationToken);
            }
        }

        private async Task<CrawlQueueItem?> GetNextPendingQueueItemAsync(Guid crawlJobId, CancellationToken cancellationToken)
        {
            return await context.CrawlQueueItems
                .Where(x => x.CrawlJobId == crawlJobId && x.Status == CrawlQueueStatus.Pending)
                .OrderBy(x => x.DepthLevel)
                .ThenBy(x => x.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);
        }

        private async Task ProcessQueueItemAsync(
            CrawlJob crawlJob,
            CrawlQueueItem queueItem,
            CancellationToken cancellationToken)
        {
            queueItem.Status = CrawlQueueStatus.InProgress;
            queueItem.AttemptCount++;

            await AddLogAsync(crawlJob.Id, CrawlLogLevel.Info, $"Fetching {queueItem.Url}.", cancellationToken);

            var fetchResult = await pageFetcher.FetchAsync(queueItem.Url, cancellationToken);

            if (!fetchResult.IsSuccess || string.IsNullOrWhiteSpace(fetchResult.Content))
            {
                queueItem.Status = CrawlQueueStatus.Failed;
                queueItem.ProcessedAt = DateTime.UtcNow;
                queueItem.ErrorMessage = fetchResult.ErrorMessage ?? $"HTTP request failed with status code {fetchResult.StatusCode}.";
                crawlJob.PagesFailed++;

                await AddLogAsync(crawlJob.Id, CrawlLogLevel.Warning, $"Failed to fetch {queueItem.Url}: {queueItem.ErrorMessage}", cancellationToken);
                return;
            }

            var parsedPage = htmlParser.Parse(fetchResult.Url, fetchResult.Content);
            var crawledPage = new CrawledPage
            {
                Id = Guid.NewGuid(),
                CrawlJobId = crawlJob.Id,
                Url = fetchResult.Url,
                Title = parsedPage.Title,
                Content = parsedPage.TextContent,
                StatusCode = fetchResult.StatusCode,
                DepthLevel = queueItem.DepthLevel,
                CrawledAt = DateTime.UtcNow,
                ResponseTimeMs = fetchResult.ResponseTimeMs
            };

            foreach (var parsedLink in parsedPage.Links)
            {
                crawledPage.Links.Add(new CrawledLink
                {
                    Id = Guid.NewGuid(),
                    SourceUrl = parsedLink.SourceUrl,
                    TargetUrl = parsedLink.TargetUrl,
                    AnchorText = parsedLink.AnchorText,
                    IsExternal = parsedLink.IsExternal,
                    DepthLevel = queueItem.DepthLevel + 1,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await context.CrawledPages.AddAsync(crawledPage, cancellationToken);

            queueItem.Status = CrawlQueueStatus.Completed;
            queueItem.ProcessedAt = DateTime.UtcNow;
            crawlJob.PagesCrawled++;

            await EnqueueDiscoveredLinksAsync(crawlJob, queueItem, parsedPage.Links, cancellationToken);
            await AddLogAsync(crawlJob.Id, CrawlLogLevel.Info, $"Crawled {queueItem.Url}. Found {parsedPage.Links.Count} links.", cancellationToken);
        }

        private async Task EnqueueDiscoveredLinksAsync(
            CrawlJob crawlJob,
            CrawlQueueItem sourceQueueItem,
            IReadOnlyCollection<ParsedLink> links,
            CancellationToken cancellationToken)
        {
            if (sourceQueueItem.DepthLevel >= crawlJob.MaxDepth || crawlJob.PagesFound >= crawlJob.MaxPages)
            {
                return;
            }

            foreach (var link in links)
            {
                if (crawlJob.PagesFound >= crawlJob.MaxPages)
                {
                    break;
                }

                if (crawlJob.StayWithinDomain && link.IsExternal)
                {
                    continue;
                }

                var alreadyQueued = await context.CrawlQueueItems
                    .AnyAsync(x => x.CrawlJobId == crawlJob.Id && x.Url == link.TargetUrl, cancellationToken);

                var alreadyCrawled = await context.CrawledPages
                    .AnyAsync(x => x.CrawlJobId == crawlJob.Id && x.Url == link.TargetUrl, cancellationToken);

                if (alreadyQueued || alreadyCrawled)
                {
                    continue;
                }

                await context.CrawlQueueItems.AddAsync(new CrawlQueueItem
                {
                    Id = Guid.NewGuid(),
                    CrawlJobId = crawlJob.Id,
                    Url = link.TargetUrl,
                    DepthLevel = sourceQueueItem.DepthLevel + 1,
                    Status = CrawlQueueStatus.Pending,
                    DiscoveredFromUrl = sourceQueueItem.Url,
                    CreatedAt = DateTime.UtcNow
                }, cancellationToken);

                crawlJob.PagesFound++;
            }
        }

        private async Task AddLogAsync(Guid crawlJobId, CrawlLogLevel level, string message, CancellationToken cancellationToken)
        {
            await context.CrawlLogs.AddAsync(new CrawlLog
            {
                Id = Guid.NewGuid(),
                CrawlJobId = crawlJobId,
                Level = level,
                Message = message,
                CreatedAt = DateTime.UtcNow
            }, cancellationToken);
        }
    }
}
