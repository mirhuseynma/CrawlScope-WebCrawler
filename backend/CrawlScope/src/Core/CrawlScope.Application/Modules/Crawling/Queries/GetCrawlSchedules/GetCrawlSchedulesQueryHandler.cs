using CrawlScope.Application.Abstractions.Persistence;
using CrawlScope.Application.Modules.Crawling.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CrawlScope.Application.Modules.Crawling.Queries.GetCrawlSchedules
{
    public class GetCrawlSchedulesQueryHandler(IAppDbContext context)
        : IRequestHandler<GetCrawlSchedulesQuery, IEnumerable<CrawlScheduleListItemDto>>
    {
        public async Task<IEnumerable<CrawlScheduleListItemDto>> Handle(GetCrawlSchedulesQuery request, CancellationToken cancellationToken)
        {
            return await context.CrawlSchedules
                .AsNoTracking()
                .OrderBy(x => x.NextRunAt)
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
                })
                .ToListAsync(cancellationToken);
        }
    }
}
