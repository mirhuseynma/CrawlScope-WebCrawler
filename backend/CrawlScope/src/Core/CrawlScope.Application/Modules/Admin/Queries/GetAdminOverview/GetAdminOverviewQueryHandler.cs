using CrawlScope.Application.Abstractions.Persistence;
using CrawlScope.Application.Modules.Admin.DTOs;
using CrawlScope.Domain.Modules.Crawling.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CrawlScope.Application.Modules.Admin.Queries.GetAdminOverview
{
    public class GetAdminOverviewQueryHandler(IAppDbContext context)
        : IRequestHandler<GetAdminOverviewQuery, AdminOverviewDto>
    {
        public async Task<AdminOverviewDto> Handle(GetAdminOverviewQuery request, CancellationToken cancellationToken)
        {
            var statusCounts = await context.CrawlJobs
                .AsNoTracking()
                .GroupBy(x => x.Status)
                .Select(x => new { Status = x.Key, Count = x.Count() })
                .ToListAsync(cancellationToken);
            var statusCountMap = statusCounts.ToDictionary(x => x.Status, x => x.Count);

            var totalPages = await context.CrawledPages.CountAsync(cancellationToken);
            var failedPages = await context.CrawledPages.CountAsync(
                x => x.StatusCode.HasValue && x.StatusCode.Value >= 400,
                cancellationToken);
            var importantJobs = await context.CrawlJobs.CountAsync(x => x.IsImportant, cancellationToken);
            var totalExports = await context.ExportFiles.CountAsync(cancellationToken);
            var totalExportSizeBytes = await context.ExportFiles.SumAsync(x => (long?)x.FileSizeBytes, cancellationToken) ?? 0;

            var recentJobs = await context.CrawlJobs
                .AsNoTracking()
                .OrderByDescending(x => x.CreatedAt)
                .Take(5)
                .Select(x => new AdminOverviewJobDto
                {
                    Id = x.Id,
                    TargetUrl = x.TargetUrl,
                    Status = x.Status.ToString(),
                    PagesCrawled = x.PagesCrawled,
                    PagesFailed = x.PagesFailed,
                    IsImportant = x.IsImportant,
                    CreatedAt = x.CreatedAt
                })
                .ToListAsync(cancellationToken);

            var recentExports = await context.ExportFiles
                .AsNoTracking()
                .Include(x => x.CrawlJob)
                .OrderByDescending(x => x.CreatedAt)
                .Take(5)
                .Select(x => new AdminOverviewExportDto
                {
                    Id = x.Id,
                    CrawlJobId = x.CrawlJobId,
                    CrawlJobTargetUrl = x.CrawlJob.TargetUrl,
                    Format = x.Format.ToString(),
                    FileName = x.FileName,
                    FileSizeBytes = x.FileSizeBytes,
                    CreatedAt = x.CreatedAt
                })
                .ToListAsync(cancellationToken);

            var problemJobs = await context.CrawlJobs
                .AsNoTracking()
                .Where(x => x.Status == CrawlJobStatus.Failed || x.PagesFailed > 0)
                .OrderByDescending(x => x.Status == CrawlJobStatus.Failed)
                .ThenByDescending(x => x.PagesFailed)
                .ThenByDescending(x => x.CreatedAt)
                .Take(5)
                .Select(x => new AdminOverviewJobDto
                {
                    Id = x.Id,
                    TargetUrl = x.TargetUrl,
                    Status = x.Status.ToString(),
                    PagesCrawled = x.PagesCrawled,
                    PagesFailed = x.PagesFailed,
                    IsImportant = x.IsImportant,
                    CreatedAt = x.CreatedAt
                })
                .ToListAsync(cancellationToken);

            return new AdminOverviewDto
            {
                Totals = new AdminOverviewTotalsDto
                {
                    TotalJobs = statusCounts.Sum(x => x.Count),
                    PendingJobs = statusCountMap.GetValueOrDefault(CrawlJobStatus.Pending),
                    InProgressJobs = statusCountMap.GetValueOrDefault(CrawlJobStatus.InProgress),
                    CompletedJobs = statusCountMap.GetValueOrDefault(CrawlJobStatus.Completed),
                    FailedJobs = statusCountMap.GetValueOrDefault(CrawlJobStatus.Failed),
                    CanceledJobs = statusCountMap.GetValueOrDefault(CrawlJobStatus.Canceled),
                    ImportantJobs = importantJobs,
                    TotalPages = totalPages,
                    FailedPages = failedPages,
                    TotalExports = totalExports,
                    TotalExportSizeBytes = totalExportSizeBytes
                },
                StatusDistribution = statusCounts
                    .OrderBy(x => x.Status)
                    .Select(x => new AdminStatusCountDto
                    {
                        Status = x.Status.ToString(),
                        Count = x.Count
                    })
                    .ToList(),
                RecentJobs = recentJobs,
                RecentExports = recentExports,
                ProblemJobs = problemJobs
            };
        }

    }
}
