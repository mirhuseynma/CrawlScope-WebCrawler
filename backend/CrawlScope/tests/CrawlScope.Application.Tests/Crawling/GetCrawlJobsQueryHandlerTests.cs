using CrawlScope.Application.Modules.Crawling.Queries.GetCrawlJobs;
using CrawlScope.Application.Tests.Common;
using CrawlScope.Domain.Modules.Crawling.Enums;
using CrawlScope.Domain.Modules.Crawling.Models;

namespace CrawlScope.Application.Tests.Crawling;

public class GetCrawlJobsQueryHandlerTests
{
    [Fact]
    public async Task Handle_ShouldFilterByOwnerStatusSearchAndImportance()
    {
        await using var context = TestDbContextFactory.Create();
        var now = DateTime.UtcNow;

        context.CrawlJobs.AddRange(
            new CrawlJob
            {
                Id = Guid.NewGuid(),
                TargetUrl = "https://example.com",
                MaxDepth = 1,
                MaxPages = 5,
                StayWithinDomain = true,
                Status = CrawlJobStatus.Completed,
                IsImportant = true,
                CreatedBy = "user-1",
                CreatedAt = now.AddMinutes(-1),
                PagesFound = 5,
                PagesCrawled = 5
            },
            new CrawlJob
            {
                Id = Guid.NewGuid(),
                TargetUrl = "https://example.org",
                MaxDepth = 1,
                MaxPages = 5,
                StayWithinDomain = true,
                Status = CrawlJobStatus.Failed,
                IsImportant = true,
                CreatedBy = "user-1",
                CreatedAt = now.AddMinutes(-2)
            },
            new CrawlJob
            {
                Id = Guid.NewGuid(),
                TargetUrl = "https://example.com/admin",
                MaxDepth = 1,
                MaxPages = 5,
                StayWithinDomain = true,
                Status = CrawlJobStatus.Completed,
                IsImportant = true,
                CreatedBy = "admin",
                CreatedAt = now
            });
        await context.SaveChangesAsync();

        var handler = new GetCrawlJobsQueryHandler(context, TestMapperFactory.Create());
        var query = new GetCrawlJobsQuery(
            Search: "example.com",
            Status: "completed",
            ImportantOnly: true,
            PageNumber: 1,
            PageSize: 10,
            RequestingUserId: "user-1",
            IncludeAllUsers: false);

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.Single(result.Items);
        Assert.Equal("https://example.com", result.Items[0].TargetUrl);
        Assert.Equal("Completed", result.Items[0].Status);
    }
}
