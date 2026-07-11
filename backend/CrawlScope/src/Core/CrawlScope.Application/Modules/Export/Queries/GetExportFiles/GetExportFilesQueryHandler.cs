namespace CrawlScope.Application.Modules.Export.Queries.GetExportFiles
{
    public class GetExportFilesQueryHandler(IAppDbContext context)
        : IRequestHandler<GetExportFilesQuery, PagedResult<ExportFileListItemDto>>
    {
        public async Task<PagedResult<ExportFileListItemDto>> Handle(GetExportFilesQuery request, CancellationToken cancellationToken)
        {
            var query = context.ExportFiles
                .AsNoTracking()
                .Include(x => x.CrawlJob)
                .WhereIf(!request.IncludeAllUsers, x => x.CreatedByUserId == request.RequestingUserId);

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.Trim();
                query = query.Where(x => x.FileName.Contains(search) || x.CrawlJob.TargetUrl.Contains(search));
            }

            if (!string.IsNullOrWhiteSpace(request.Format)
                && Enum.TryParse<ExportFormat>(request.Format, ignoreCase: true, out var format))
            {
                query = query.Where(x => x.Format == format);
            }

            var projectedQuery = query
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new ExportFileListItemDto
                {
                    Id = x.Id,
                    CrawlJobId = x.CrawlJobId,
                    CrawlJobTargetUrl = x.CrawlJob.TargetUrl,
                    Format = x.Format.ToString(),
                    FileName = x.FileName,
                    FileSizeBytes = x.FileSizeBytes,
                    CreatedAt = x.CreatedAt
                });

            return await PagedResult<ExportFileListItemDto>.CreateAsync(
                projectedQuery,
                request.PageNumber,
                request.PageSize,
                cancellationToken);
        }
    }
}
