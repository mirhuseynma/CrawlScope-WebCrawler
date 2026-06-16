using CrawlScope.Domain.Modules.Crawling.Models;
using CrawlScope.Domain.Modules.Export.Models;
using Microsoft.EntityFrameworkCore;

namespace CrawlScope.Application.Abstractions.Persistence
{
    public interface IAppDbContext
    {
        DbSet<CrawlJob> CrawlJobs { get; }
        DbSet<CrawlQueueItem> CrawlQueueItems { get; }
        DbSet<CrawledPage> CrawledPages { get; }
        DbSet<CrawledLink> CrawledLinks { get; }
        DbSet<CrawlLog> CrawlLogs { get; }
        DbSet<ExportFile> ExportFiles { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
