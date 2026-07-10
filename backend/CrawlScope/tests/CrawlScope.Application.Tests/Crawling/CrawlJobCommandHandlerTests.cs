using CrawlScope.Application.Common.Exceptions;
using CrawlScope.Application.Modules.Crawling.Commands.DeleteCrawlJob;
using CrawlScope.Application.Modules.Crawling.Commands.ToggleCrawlJobImportance;
using CrawlScope.Application.Tests.Common;
using CrawlScope.Domain.Modules.Crawling.Enums;
using CrawlScope.Domain.Modules.Crawling.Models;
using CrawlScope.Domain.Modules.Export.Models;

namespace CrawlScope.Application.Tests.Crawling;

public class CrawlJobCommandHandlerTests
{
    [Fact]
    public async Task ToggleImportance_WhenJobExists_ShouldFlipAndPersistValue()
    {
        await using var context = TestDbContextFactory.Create();
        var jobId = Guid.NewGuid();
        context.CrawlJobs.Add(new CrawlJob
        {
            Id = jobId,
            TargetUrl = "https://example.com",
            MaxDepth = 1,
            MaxPages = 5,
            StayWithinDomain = true,
            CreatedBy = "user-1",
            IsImportant = false
        });
        await context.SaveChangesAsync();

        var handler = new ToggleCrawlJobImportanceCommandHandler(context);

        var isImportant = await handler.Handle(new ToggleCrawlJobImportanceCommand(jobId), CancellationToken.None);

        Assert.True(isImportant);
        Assert.True(context.CrawlJobs.Single(x => x.Id == jobId).IsImportant);
    }

    [Fact]
    public async Task Delete_WhenRequesterDoesNotOwnJob_ShouldThrowNotFound()
    {
        await using var context = TestDbContextFactory.Create();
        var jobId = Guid.NewGuid();
        context.CrawlJobs.Add(new CrawlJob
        {
            Id = jobId,
            TargetUrl = "https://example.com",
            MaxDepth = 1,
            MaxPages = 5,
            StayWithinDomain = true,
            Status = CrawlJobStatus.Completed,
            CreatedBy = "owner-user"
        });
        await context.SaveChangesAsync();

        var handler = new DeleteCrawlJobCommandHandler(context, new FakeExportFileStorage());
        var command = new DeleteCrawlJobCommand(
            Id: jobId,
            RequestingUserId: "another-user",
            IncludeAllUsers: false);

        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Delete_WhenJobHasExportFiles_ShouldDeleteRecordAndStorageFiles()
    {
        await using var context = TestDbContextFactory.Create();
        var jobId = Guid.NewGuid();
        context.CrawlJobs.Add(new CrawlJob
        {
            Id = jobId,
            TargetUrl = "https://example.com",
            MaxDepth = 1,
            MaxPages = 5,
            StayWithinDomain = true,
            Status = CrawlJobStatus.Completed,
            CreatedBy = "owner-user",
            ExportFiles =
            [
                new ExportFile
                {
                    Id = Guid.NewGuid(),
                    CrawlJobId = jobId,
                    FileName = "crawl.json",
                    FilePath = "exports/crawl.json",
                    Format = ExportFormat.Json,
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUserId = "owner-user"
                }
            ]
        });
        await context.SaveChangesAsync();

        var storage = new FakeExportFileStorage();
        var handler = new DeleteCrawlJobCommandHandler(context, storage);
        var command = new DeleteCrawlJobCommand(
            Id: jobId,
            RequestingUserId: "owner-user",
            IncludeAllUsers: false);

        await handler.Handle(command, CancellationToken.None);

        Assert.Empty(context.CrawlJobs);
        Assert.Equal(["exports/crawl.json"], storage.DeletedPaths);
    }
}
