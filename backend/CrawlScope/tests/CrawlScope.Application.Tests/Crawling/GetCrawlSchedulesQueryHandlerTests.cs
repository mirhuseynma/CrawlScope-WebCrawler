using CrawlScope.Application.Modules.Crawling.Queries.GetCrawlSchedules;
using CrawlScope.Application.Tests.Common;
using CrawlScope.Domain.Modules.Crawling.Models;

namespace CrawlScope.Application.Tests.Crawling;

public class GetCrawlSchedulesQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenFiltersAreProvided_ShouldReturnMatchingPagedSchedules()
    {
        await using var context = TestDbContextFactory.Create();
        var now = DateTime.UtcNow;

        context.CrawlSchedules.AddRange(
            new CrawlSchedule
            {
                Id = Guid.NewGuid(),
                TargetUrl = "https://example.com",
                MaxDepth = 1,
                MaxPages = 10,
                StayWithinDomain = true,
                IntervalMinutes = 60,
                IsEnabled = true,
                CreatedAt = now.AddDays(-3),
                NextRunAt = now.AddHours(3),
                CreatedBy = "admin"
            },
            new CrawlSchedule
            {
                Id = Guid.NewGuid(),
                TargetUrl = "https://docs.example.com",
                MaxDepth = 2,
                MaxPages = 20,
                StayWithinDomain = true,
                IntervalMinutes = 30,
                IsEnabled = true,
                CreatedAt = now.AddDays(-2),
                NextRunAt = now.AddHours(1),
                CreatedBy = "admin"
            },
            new CrawlSchedule
            {
                Id = Guid.NewGuid(),
                TargetUrl = "https://example.org",
                MaxDepth = 1,
                MaxPages = 5,
                StayWithinDomain = false,
                IntervalMinutes = 120,
                IsEnabled = false,
                CreatedAt = now.AddDays(-1),
                NextRunAt = now.AddHours(2),
                CreatedBy = "admin"
            });
        await context.SaveChangesAsync();

        var handler = new GetCrawlSchedulesQueryHandler(context);
        var query = new GetCrawlSchedulesQuery("example", true, PageNumber: 1, PageSize: 1);

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.Equal(2, result.TotalCount);
        Assert.Equal(2, result.TotalPages);
        Assert.True(result.HasNextPage);
        Assert.Single(result.Items);
        Assert.Equal("https://docs.example.com", result.Items[0].TargetUrl);
    }

    [Fact]
    public async Task Handle_WhenPageSizeIsTooLarge_ShouldClampPageSize()
    {
        await using var context = TestDbContextFactory.Create();

        for (var index = 0; index < 3; index++)
        {
            context.CrawlSchedules.Add(new CrawlSchedule
            {
                Id = Guid.NewGuid(),
                TargetUrl = $"https://site-{index}.com",
                MaxDepth = 1,
                MaxPages = 10,
                StayWithinDomain = true,
                IntervalMinutes = 60,
                IsEnabled = true,
                CreatedAt = DateTime.UtcNow.AddMinutes(-index),
                NextRunAt = DateTime.UtcNow.AddMinutes(index),
                CreatedBy = "admin"
            });
        }

        await context.SaveChangesAsync();

        var handler = new GetCrawlSchedulesQueryHandler(context);
        var query = new GetCrawlSchedulesQuery(null, null, PageNumber: -5, PageSize: 250);

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.Equal(1, result.PageNumber);
        Assert.Equal(100, result.PageSize);
        Assert.Equal(3, result.TotalCount);
        Assert.Equal(3, result.Items.Count);
    }
}
