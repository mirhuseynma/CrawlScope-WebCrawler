
namespace CrawlScope.Application.Tests.Crawling;

public class GetBrokenLinksQueryHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnOnlyFailedLinksMatchingFilters()
    {
        await using var context = TestDbContextFactory.Create();
        var jobId = Guid.NewGuid();
        var job = new CrawlJob
        {
            Id = jobId,
            TargetUrl = "https://example.com",
            MaxDepth = 2,
            MaxPages = 20,
            StayWithinDomain = true,
            CreatedBy = "user-1"
        };

        context.CrawlJobs.Add(job);
        context.CrawlQueueItems.AddRange(
            new CrawlQueueItem
            {
                Id = Guid.NewGuid(),
                CrawlJobId = jobId,
                CrawlJob = job,
                Url = "https://external.com/missing",
                Status = CrawlQueueStatus.Failed,
                StatusCode = 404,
                IsExternal = true,
                DepthLevel = 1,
                DiscoveredFromUrl = "https://example.com/page",
                AnchorText = "Missing partner page",
                CreatedAt = DateTime.UtcNow.AddMinutes(-10),
                ProcessedAt = DateTime.UtcNow.AddMinutes(-2)
            },
            new CrawlQueueItem
            {
                Id = Guid.NewGuid(),
                CrawlJobId = jobId,
                CrawlJob = job,
                Url = "https://example.com/internal-error",
                Status = CrawlQueueStatus.Failed,
                StatusCode = 500,
                IsExternal = false,
                DepthLevel = 1,
                DiscoveredFromUrl = "https://example.com/page",
                AnchorText = "Internal problem",
                CreatedAt = DateTime.UtcNow.AddMinutes(-9),
                ProcessedAt = DateTime.UtcNow.AddMinutes(-1)
            },
            new CrawlQueueItem
            {
                Id = Guid.NewGuid(),
                CrawlJobId = jobId,
                CrawlJob = job,
                Url = "https://external.com/ok",
                Status = CrawlQueueStatus.Completed,
                StatusCode = 200,
                IsExternal = true,
                DepthLevel = 1,
                CreatedAt = DateTime.UtcNow.AddMinutes(-8),
                ProcessedAt = DateTime.UtcNow
            });
        await context.SaveChangesAsync();

        var handler = new GetBrokenLinksQueryHandler(context);
        var query = new GetBrokenLinksQuery(
            jobId,
            Search: "partner",
            StatusCode: 404,
            ExternalOnly: true,
            PageNumber: 1,
            PageSize: 10,
            RequestingUserId: "user-1",
            IncludeAllUsers: false);

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.Single(result.Items);
        Assert.Equal(1, result.TotalCount);
        Assert.Equal("https://external.com/missing", result.Items[0].TargetUrl);
        Assert.Equal("https://example.com/page", result.Items[0].SourceUrl);
    }

    [Fact]
    public async Task Handle_WhenRequesterDoesNotOwnJob_ShouldReturnNoLinks()
    {
        await using var context = TestDbContextFactory.Create();
        var jobId = Guid.NewGuid();
        var job = new CrawlJob
        {
            Id = jobId,
            TargetUrl = "https://private.example.com",
            MaxDepth = 1,
            MaxPages = 5,
            StayWithinDomain = true,
            CreatedBy = "owner-user"
        };

        context.CrawlJobs.Add(job);
        context.CrawlQueueItems.Add(new CrawlQueueItem
        {
            Id = Guid.NewGuid(),
            CrawlJobId = jobId,
            CrawlJob = job,
            Url = "https://private.example.com/not-found",
            Status = CrawlQueueStatus.Failed,
            StatusCode = 404,
            IsExternal = false,
            DepthLevel = 0,
            CreatedAt = DateTime.UtcNow,
            ProcessedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var handler = new GetBrokenLinksQueryHandler(context);
        var query = new GetBrokenLinksQuery(
            jobId,
            Search: null,
            StatusCode: null,
            ExternalOnly: null,
            PageNumber: 1,
            PageSize: 10,
            RequestingUserId: "another-user",
            IncludeAllUsers: false);

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
    }
}
