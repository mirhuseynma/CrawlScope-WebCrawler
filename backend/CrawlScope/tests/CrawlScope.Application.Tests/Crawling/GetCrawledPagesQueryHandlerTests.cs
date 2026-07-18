
namespace CrawlScope.Application.Tests.Crawling;

public class GetCrawledPagesQueryHandlerTests
{
    [Fact]
    public async Task Handle_ShouldFilterPagesByOwnerJobStatusDepthAndSearch()
    {
        await using var context = TestDbContextFactory.Create();
        var ownedJobId = Guid.NewGuid();
        var otherJobId = Guid.NewGuid();
        var ownedJob = new CrawlJob
        {
            Id = ownedJobId,
            TargetUrl = "https://example.com",
            MaxDepth = 2,
            MaxPages = 20,
            StayWithinDomain = true,
            CreatedBy = "user-1"
        };
        var otherJob = new CrawlJob
        {
            Id = otherJobId,
            TargetUrl = "https://private.example.com",
            MaxDepth = 2,
            MaxPages = 20,
            StayWithinDomain = true,
            CreatedBy = "user-2"
        };

        context.CrawlJobs.AddRange(ownedJob, otherJob);
        context.CrawledPages.AddRange(
            new CrawledPage
            {
                Id = Guid.NewGuid(),
                CrawlJobId = ownedJobId,
                CrawlJob = ownedJob,
                Url = "https://example.com/docs",
                Title = "API documentation",
                Content = "Useful crawl scope documentation",
                StatusCode = 200,
                DepthLevel = 1,
                CrawledAt = DateTime.UtcNow.AddMinutes(-1),
                Links =
                [
                    new CrawledLink
                    {
                        Id = Guid.NewGuid(),
                        SourceUrl = "https://example.com/docs",
                        TargetUrl = "https://example.com/about",
                        IsExternal = false,
                        DepthLevel = 1,
                        CreatedAt = DateTime.UtcNow
                    },
                    new CrawledLink
                    {
                        Id = Guid.NewGuid(),
                        SourceUrl = "https://example.com/docs",
                        TargetUrl = "https://external.com",
                        IsExternal = true,
                        DepthLevel = 1,
                        CreatedAt = DateTime.UtcNow
                    }
                ]
            },
            new CrawledPage
            {
                Id = Guid.NewGuid(),
                CrawlJobId = ownedJobId,
                CrawlJob = ownedJob,
                Url = "https://example.com/not-found",
                Title = "Missing",
                StatusCode = 404,
                DepthLevel = 1,
                CrawledAt = DateTime.UtcNow
            },
            new CrawledPage
            {
                Id = Guid.NewGuid(),
                CrawlJobId = otherJobId,
                CrawlJob = otherJob,
                Url = "https://private.example.com/docs",
                Title = "Private docs",
                Content = "documentation",
                StatusCode = 200,
                DepthLevel = 1,
                CrawledAt = DateTime.UtcNow
            });
        await context.SaveChangesAsync();

        var handler = new GetCrawledPagesQueryHandler(context);
        var query = new GetCrawledPagesQuery(
            CrawlJobId: ownedJobId,
            Search: "documentation",
            StatusCode: 200,
            DepthLevel: 1,
            PageNumber: 1,
            PageSize: 10,
            RequestingUserId: "user-1",
            IncludeAllUsers: false);

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.Single(result.Items);
        Assert.Equal("https://example.com/docs", result.Items[0].Url);
        Assert.Equal(1, result.Items[0].InternalLinksCount);
        Assert.Equal(1, result.Items[0].ExternalLinksCount);
    }
}
