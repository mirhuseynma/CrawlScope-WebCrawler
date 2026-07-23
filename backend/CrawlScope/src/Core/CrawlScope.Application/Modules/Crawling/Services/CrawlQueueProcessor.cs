using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace CrawlScope.Application.Modules.Crawling.Services
{
    public class CrawlQueueProcessor(
        IAppDbContext context,
        IPageFetcherFactory pageFetcherFactory,
        IHtmlParser htmlParser,
        ILogger<CrawlQueueProcessor> logger) : ICrawlQueueProcessor
    {
        public async Task ProcessAsync(
            Guid crawlJobId,
            CancellationToken cancellationToken = default,
            CancellationToken userCancellationToken = default,
            CancellationToken timeoutCancellationToken = default)
        {
            var crawlJob = await context.CrawlJobs
                .FirstOrDefaultAsync(x => x.Id == crawlJobId, cancellationToken);

            if (crawlJob is null)
            {
                throw new NotFoundException($"Crawl job with ID {crawlJobId} not found.");
            }

            // Already canceled while sitting in the queue — skip silently
            if (crawlJob.Status == CrawlJobStatus.Canceled)
            {
                return;
            }

            try
            {
                if (crawlJob.Status != CrawlJobStatus.InProgress)
                {
                    crawlJob.Status = CrawlJobStatus.InProgress;
                    crawlJob.StartedAt = DateTime.UtcNow;
                    await ExecuteWithDiagnosticSaveAsync(cancellationToken);
                }
                while (!cancellationToken.IsCancellationRequested)
                {
                    logger.LogInformation("--- START MAIN LOOP ITERATION ---");
                    
                    var pendingCount = await context.CrawlQueueItems.CountAsync(x => x.CrawlJobId == crawlJobId && x.Status == CrawlQueueStatus.Pending, cancellationToken);
                    var inProgressCount = await context.CrawlQueueItems.CountAsync(x => x.CrawlJobId == crawlJobId && x.Status == CrawlQueueStatus.InProgress, cancellationToken);
                    var completedCount = await context.CrawlQueueItems.CountAsync(x => x.CrawlJobId == crawlJobId && x.Status == CrawlQueueStatus.Completed, cancellationToken);
                    var failedCount = await context.CrawlQueueItems.CountAsync(x => x.CrawlJobId == crawlJobId && x.Status == CrawlQueueStatus.Failed, cancellationToken);
                    
                    logger.LogInformation("Queue counts - Pending: {Pending}, InProgress: {InProgress}, Completed: {Completed}, Failed: {Failed}", pendingCount, inProgressCount, completedCount, failedCount);
                    logger.LogInformation("Calling GetNextPendingQueueItemAsync");

                    var queueItem = await GetNextPendingQueueItemAsync(crawlJobId, cancellationToken);
                    
                    if (queueItem == null)
                    {
                        logger.LogInformation("GetNextPendingQueueItemAsync returned null. Checking if there are really zero pending items.");
                        if (pendingCount > 0) 
                        {
                            logger.LogWarning("WARNING: GetNextPendingQueueItemAsync returned null BUT there are {Count} pending items! Query might be failing or locked.", pendingCount);
                        }
                    }
                    else
                    {
                        logger.LogInformation("GetNextPendingQueueItemAsync returned Item Id: {Id}, Url: {Url}, Status: {Status}, Job PagesCrawled: {PagesCrawled}", queueItem.Id, queueItem.Url, queueItem.Status, crawlJob.PagesCrawled);
                    }

                    if (queueItem is null || crawlJob.PagesCrawled >= crawlJob.MaxPages)
                    {
                        logger.LogInformation("Breaking loop. QueueItem is null? {IsNull}. PagesCrawled: {Crawled}/{MaxPages}", queueItem is null, crawlJob.PagesCrawled, crawlJob.MaxPages);
                        break;
                    }

                    logger.LogInformation("Calling ProcessQueueItemAsync");
                    await ProcessQueueItemAsync(crawlJob, queueItem, cancellationToken);
                    logger.LogInformation("ProcessQueueItemAsync returned. Calling SaveChanges");
                    await ExecuteWithDiagnosticSaveAsync(cancellationToken);
                    logger.LogInformation("--- END MAIN LOOP ITERATION ---");
                }

                crawlJob.Status = CrawlJobStatus.Completed;
                crawlJob.FinishedAt = DateTime.UtcNow;

                await AddLogAsync(crawlJob.Id, CrawlLogLevel.Info, "Crawl job completed.", cancellationToken);
                await ExecuteWithDiagnosticSaveAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                logger.LogInformation("OperationCanceledException caught.");
                if (userCancellationToken.IsCancellationRequested)
                {
                    // User clicked Cancel
                    logger.LogInformation("Canceled by user.");
                    crawlJob.Status = CrawlJobStatus.Canceled;
                    crawlJob.FinishedAt = DateTime.UtcNow;
                    await AddLogAsync(crawlJob.Id, CrawlLogLevel.Warning, "Crawl job was canceled by user.", CancellationToken.None);
                    await ExecuteWithDiagnosticSaveAsync(CancellationToken.None);
                }
                else if (timeoutCancellationToken.IsCancellationRequested)
                {
                    // 10-minute time limit exceeded — save partial results
                    logger.LogInformation("Timeout cancellation requested.");
                    crawlJob.Status = CrawlJobStatus.Completed;
                    crawlJob.FinishedAt = DateTime.UtcNow;
                    await AddLogAsync(crawlJob.Id, CrawlLogLevel.Warning, "Crawl job stopped: 10-minute time limit exceeded. Partial results saved.", CancellationToken.None);
                    await ExecuteWithDiagnosticSaveAsync(CancellationToken.None);
                }
                else
                {
                    // Service is shutting down — do not update status, Recovery will re-enqueue
                    logger.LogInformation("Service shutting down cancellation.");
                    throw;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Generic exception caught in ProcessAsync");
                crawlJob.Status = CrawlJobStatus.Failed;
                crawlJob.FinishedAt = DateTime.UtcNow;
                crawlJob.ErrorMessage = ex.Message;

                await AddLogAsync(crawlJob.Id, CrawlLogLevel.Error, $"Crawl job failed: {ex.Message}", cancellationToken);
                await ExecuteWithDiagnosticSaveAsync(cancellationToken);
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
            logger.LogInformation("START ProcessQueueItem for URL: {Url}", queueItem.Url);
            queueItem.Status = CrawlQueueStatus.InProgress;
            queueItem.AttemptCount++;

            await AddLogAsync(crawlJob.Id, CrawlLogLevel.Info, $"Fetching {queueItem.Url}.", cancellationToken);

            logger.LogInformation("FetchAsync started for URL: {Url}", queueItem.Url);
            var pageFetcher = pageFetcherFactory.Create(crawlJob.Type);
            var fetchResult = await pageFetcher.FetchAsync(queueItem.Url, cancellationToken);
            logger.LogInformation("FetchAsync completed for URL: {Url}. Success: {IsSuccess}, Status: {Status}", queueItem.Url, fetchResult.IsSuccess, fetchResult.StatusCode);

            if (crawlJob.Type == CrawlType.Fast && ShouldRetryWithBrowser(fetchResult))
            {
                await AddLogAsync(
                    crawlJob.Id,
                    CrawlLogLevel.Warning,
                    $"Standard crawl was blocked for {queueItem.Url} with status code {fetchResult.StatusCode}. Retrying with Browser crawl.",
                    cancellationToken);

                logger.LogInformation("Retrying FetchAsync with Browser for URL: {Url}", queueItem.Url);
                pageFetcher = pageFetcherFactory.Create(CrawlType.Dynamic);
                fetchResult = await pageFetcher.FetchAsync(queueItem.Url, cancellationToken);
                logger.LogInformation("Browser FetchAsync completed for URL: {Url}. Success: {IsSuccess}", queueItem.Url, fetchResult.IsSuccess);
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
                logger.LogInformation("ProcessQueueItem completed (Failed) for URL: {Url}", queueItem.Url);
                return;
            }

            var normalizedFetchUrl = UrlNormalizer.Normalize(fetchResult.Url);
            logger.LogInformation("Parse started for URL: {Url}", normalizedFetchUrl);
            var parsedPage = htmlParser.Parse(normalizedFetchUrl, fetchResult.Content);
            logger.LogInformation("Parse completed for URL: {Url}. Found {Count} links.", normalizedFetchUrl, parsedPage.Links.Count);

            logger.LogInformation("Saving CrawledPage for URL: {Url}", normalizedFetchUrl);
            var crawledPage = new CrawledPage
            {
                Id = Guid.NewGuid(),
                CrawlJobId = crawlJob.Id,
                Url = normalizedFetchUrl,
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

            logger.LogInformation("Enqueue started for URL: {Url}", queueItem.Url);
            await EnqueueDiscoveredLinksAsync(crawlJob, queueItem, parsedPage.Links, cancellationToken);
            logger.LogInformation("Enqueue completed for URL: {Url}", queueItem.Url);
            await AddLogAsync(crawlJob.Id, CrawlLogLevel.Info, $"Crawled {queueItem.Url}. Found {parsedPage.Links.Count} links.", cancellationToken);
            
            logger.LogInformation("ProcessQueueItem completed for URL: {Url}", queueItem.Url);
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

        private async Task ExecuteWithDiagnosticSaveAsync(CancellationToken cancellationToken)
        {
            var trackedCount = 0;
            if (context is Microsoft.EntityFrameworkCore.DbContext dbContext)
            {
                trackedCount = dbContext.ChangeTracker.Entries().Count();
            }
            logger.LogInformation("entering SaveChanges. Tracked entities: {Count}", trackedCount);
            
            var sw = Stopwatch.StartNew();
            await context.SaveChangesAsync(cancellationToken);
            sw.Stop();
            
            if (sw.ElapsedMilliseconds > 3000)
            {
                logger.LogWarning("leaving SaveChanges (SLOW). elapsed milliseconds: {Elapsed}", sw.ElapsedMilliseconds);
            }
            else
            {
                logger.LogInformation("leaving SaveChanges. elapsed milliseconds: {Elapsed}", sw.ElapsedMilliseconds);
            }
        }
    }
}
