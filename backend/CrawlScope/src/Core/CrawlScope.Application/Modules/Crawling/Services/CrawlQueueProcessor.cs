
namespace CrawlScope.Application.Modules.Crawling.Services
{
    public class CrawlQueueProcessor(
        IAppDbContext context,
        CrawlScope.Application.Abstractions.Crawling.Services.IPageFetcherFactory pageFetcherFactory,
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
                if (crawlJob.Status != CrawlJobStatus.InProgress)
                {
                    crawlJob.Status = CrawlJobStatus.InProgress;
                    crawlJob.StartedAt = DateTime.UtcNow;
                    await context.SaveChangesAsync(cancellationToken);
                }
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

            var pageFetcher = pageFetcherFactory.Create(crawlJob.Type);
            var fetchResult = await pageFetcher.FetchAsync(queueItem.Url, cancellationToken);
            if (crawlJob.Type == CrawlType.Fast && ShouldRetryWithBrowser(fetchResult))
            {
                await AddLogAsync(
                    crawlJob.Id,
                    CrawlLogLevel.Warning,
                    $"Standard crawl was blocked for {queueItem.Url} with status code {fetchResult.StatusCode}. Retrying with Browser crawl.",
                    cancellationToken);

                pageFetcher = pageFetcherFactory.Create(CrawlType.Dynamic);
                fetchResult = await pageFetcher.FetchAsync(queueItem.Url, cancellationToken);
            }

            if (!fetchResult.IsSuccess || string.IsNullOrWhiteSpace(fetchResult.Content))
            {
                queueItem.Status = CrawlQueueStatus.Failed;
                queueItem.ProcessedAt = DateTime.UtcNow;
                queueItem.ErrorMessage = GetFetchErrorMessage(fetchResult);
                queueItem.StatusCode = fetchResult.StatusCode;
                queueItem.ResponseTimeMs = fetchResult.ResponseTimeMs;
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

        private static bool ShouldRetryWithBrowser(PageFetchResult fetchResult)
        {
            return fetchResult.StatusCode is 401 or 403 or 429 or 503;
        }

        private static string GetFetchErrorMessage(PageFetchResult fetchResult)
        {
            if (!string.IsNullOrWhiteSpace(fetchResult.ErrorMessage))
            {
                return fetchResult.ErrorMessage;
            }

            return fetchResult.StatusCode switch
            {
                401 or 403 => "The origin server denied crawler access. Try Browser crawl; if it still fails, the site is blocking this server/IP or automated traffic.",
                429 => "The origin server rate-limited the crawl request.",
                503 => "The origin server returned service unavailable, often used by bot protection during challenge pages.",
                _ => $"HTTP request failed with status code {fetchResult.StatusCode}."
            };
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

            var validLinks = links
                .Where(link => !(crawlJob.StayWithinDomain && link.IsExternal))
                .Select(link => link.TargetUrl)
                .Distinct()
                .ToList();

            if (validLinks.Count == 0) return;

            var existingQueuedUrls = await context.CrawlQueueItems
                .Where(x => x.CrawlJobId == crawlJob.Id && validLinks.Contains(x.Url))
                .Select(x => x.Url)
                .ToListAsync(cancellationToken);

            var existingCrawledUrls = await context.CrawledPages
                .Where(x => x.CrawlJobId == crawlJob.Id && validLinks.Contains(x.Url))
                .Select(x => x.Url)
                .ToListAsync(cancellationToken);

            var existingUrls = new HashSet<string>(existingQueuedUrls.Concat(existingCrawledUrls));

            foreach (var link in links.Where(l => validLinks.Contains(l.TargetUrl)))
            {
                if (crawlJob.PagesFound >= crawlJob.MaxPages)
                {
                    break;
                }

                if (!existingUrls.Add(link.TargetUrl))
                {
                    continue;
                }

                await context.CrawlQueueItems.AddAsync(new CrawlQueueItem
                {
                    Id = Guid.NewGuid(),
                    CrawlJobId = crawlJob.Id,
                    Url = link.TargetUrl,
                    DepthLevel = sourceQueueItem.DepthLevel + 1,
                    DiscoveredFromUrl = sourceQueueItem.Url,
                    AnchorText = link.AnchorText,
                    IsExternal = link.IsExternal,
                    Status = CrawlQueueStatus.Pending,
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
