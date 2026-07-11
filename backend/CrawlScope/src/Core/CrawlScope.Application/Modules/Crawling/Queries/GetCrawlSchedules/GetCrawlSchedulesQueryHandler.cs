namespace CrawlScope.Application.Modules.Crawling.Queries.GetCrawlSchedules
{
    public class GetCrawlSchedulesQueryHandler(IAppDbContext context)
        : IRequestHandler<GetCrawlSchedulesQuery, PagedResult<CrawlScheduleListItemDto>>
    {
        public async Task<PagedResult<CrawlScheduleListItemDto>> Handle(GetCrawlSchedulesQuery request, CancellationToken cancellationToken)
        {
            var query = context.CrawlSchedules.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.Trim();
                query = query.Where(x => x.TargetUrl.Contains(search));
            }

            if (request.IsEnabled.HasValue)
            {
                query = query.Where(x => x.IsEnabled == request.IsEnabled.Value);
            }

            var projectedQuery = query
                .OrderByDescending(x => x.IsEnabled)
                .ThenBy(x => x.NextRunAt)
                .Select(x => new CrawlScheduleListItemDto
                {
                    Id = x.Id,
                    TargetUrl = x.TargetUrl,
                    MaxDepth = x.MaxDepth,
                    MaxPages = x.MaxPages,
                    StayWithinDomain = x.StayWithinDomain,
                    IntervalMinutes = x.IntervalMinutes,
                    IsEnabled = x.IsEnabled,
                    CreatedAt = x.CreatedAt,
                    NextRunAt = x.NextRunAt,
                    LastRunAt = x.LastRunAt,
                    LastCrawlJobId = x.LastCrawlJobId
                });

            return await PagedResult<CrawlScheduleListItemDto>.CreateAsync(
                projectedQuery,
                request.PageNumber,
                request.PageSize,
                cancellationToken);
        }
    }
}
