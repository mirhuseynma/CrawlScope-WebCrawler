
namespace CrawlScope.Application.Tests.Export;

public class ExportCommandHandlerTests
{
    [Fact]
    public async Task ExportJson_WhenJobBelongsToUser_ShouldSaveContentAndMetadata()
    {
        await using var context = TestDbContextFactory.Create();
        var jobId = Guid.NewGuid();
        var job = new CrawlJob
        {
            Id = jobId,
            TargetUrl = "https://example.com",
            MaxDepth = 1,
            MaxPages = 5,
            StayWithinDomain = true,
            CreatedBy = "user-1"
        };

        context.CrawlJobs.Add(job);
        context.CrawledPages.Add(new CrawledPage
        {
            Id = Guid.NewGuid(),
            CrawlJobId = jobId,
            CrawlJob = job,
            Url = "https://example.com",
            Title = "AzÉ™rbaycan sÉ™hifÉ™si",
            Content = "Salam dÃ¼nya",
            StatusCode = 200,
            DepthLevel = 0,
            CrawledAt = DateTime.UtcNow,
            Links =
            [
                new CrawledLink
                {
                    Id = Guid.NewGuid(),
                    SourceUrl = "https://example.com",
                    TargetUrl = "https://example.com/about",
                    AnchorText = "About",
                    IsExternal = false,
                    DepthLevel = 0,
                    CreatedAt = DateTime.UtcNow
                }
            ]
        });
        await context.SaveChangesAsync();

        var storage = new FakeExportFileStorage();
        var strategies = new[] { new FakeExportStrategy(ExportFormat.Json, "AzÉ™rbaycan sÉ™hifÉ™si Salam dÃ¼nya") };
        var handler = new ExportCrawledDataCommandHandler(context, storage, strategies);
        var command = new ExportCrawledDataCommand(jobId, ExportFormat.Json, "user-1", IncludeAllUsers: false);

        var result = await handler.Handle(command, CancellationToken.None);

        var exportFile = context.ExportFiles.Single();
        Assert.Equal(result.ExportFileId, exportFile.Id);
        Assert.Equal(ExportFormat.Json, exportFile.Format);
        Assert.Equal("user-1", exportFile.CreatedByUserId);
        var storedContent = storage.Files[exportFile.FilePath];
        Assert.Equal(exportFile.FileSizeBytes, storedContent.LongLength);

        var json = Encoding.UTF8.GetString(storedContent);
        Assert.Contains("AzÉ™rbaycan sÉ™hifÉ™si", json);
        Assert.Contains("Salam dÃ¼nya", json);
    }

    [Fact]
    public async Task ExportCsv_WhenJobDoesNotBelongToRequester_ShouldThrowNotFound()
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
            CreatedBy = "owner-user"
        });
        await context.SaveChangesAsync();

        var strategies = new[] { new FakeExportStrategy(ExportFormat.Csv, "csv") };
        var handler = new ExportCrawledDataCommandHandler(context, new FakeExportFileStorage(), strategies);
        var command = new ExportCrawledDataCommand(jobId, ExportFormat.Csv, "another-user", IncludeAllUsers: false);

        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Download_WhenExportExistsAndContentExists_ShouldReturnStoredFile()
    {
        await using var context = TestDbContextFactory.Create();
        var job = new CrawlJob
        {
            Id = Guid.NewGuid(),
            TargetUrl = "https://example.com",
            MaxDepth = 1,
            MaxPages = 5,
            StayWithinDomain = true,
            CreatedBy = "user-1"
        };
        var exportId = Guid.NewGuid();
        var filePath = "stored/export.json";
        context.CrawlJobs.Add(job);
        context.ExportFiles.Add(new ExportFile
        {
            Id = exportId,
            CrawlJobId = job.Id,
            CrawlJob = job,
            FileName = "export.json",
            FilePath = filePath,
            Format = ExportFormat.Json,
            FileSizeBytes = 2,
            CreatedAt = DateTime.UtcNow,
            CreatedByUserId = "user-1"
        });
        await context.SaveChangesAsync();

        var storage = new FakeExportFileStorage();
        storage.Files[filePath] = Encoding.UTF8.GetBytes("{}");
        var handler = new DownloadExportFileQueryHandler(context, storage);
        var query = new DownloadExportFileQuery(exportId, "user-1", IncludeAllUsers: false);

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.Equal("export.json", result.FileName);
        Assert.Equal("application/json; charset=utf-8", result.ContentType);
        
        using var reader = new StreamReader(result.ContentStream);
        var content = await reader.ReadToEndAsync();
        Assert.Equal("{}", content);
    }

    [Fact]
    public async Task Delete_WhenExportExists_ShouldRemoveMetadataAndStoredFile()
    {
        await using var context = TestDbContextFactory.Create();
        var job = new CrawlJob
        {
            Id = Guid.NewGuid(),
            TargetUrl = "https://example.com",
            MaxDepth = 1,
            MaxPages = 5,
            StayWithinDomain = true,
            CreatedBy = "user-1"
        };
        var exportId = Guid.NewGuid();
        var filePath = "stored/export.csv";
        context.CrawlJobs.Add(job);
        context.ExportFiles.Add(new ExportFile
        {
            Id = exportId,
            CrawlJobId = job.Id,
            CrawlJob = job,
            FileName = "export.csv",
            FilePath = filePath,
            Format = ExportFormat.Csv,
            FileSizeBytes = 10,
            CreatedAt = DateTime.UtcNow,
            CreatedByUserId = "user-1"
        });
        await context.SaveChangesAsync();

        var storage = new FakeExportFileStorage();
        storage.Files[filePath] = Encoding.UTF8.GetBytes("csv");
        var handler = new DeleteExportFileCommandHandler(context, storage);
        var command = new DeleteExportFileCommand(exportId, "user-1", IncludeAllUsers: false);

        await handler.Handle(command, CancellationToken.None);

        Assert.Empty(context.ExportFiles);
        Assert.Equal([filePath], storage.DeletedPaths);
        Assert.False(storage.Files.ContainsKey(filePath));
    }
}
