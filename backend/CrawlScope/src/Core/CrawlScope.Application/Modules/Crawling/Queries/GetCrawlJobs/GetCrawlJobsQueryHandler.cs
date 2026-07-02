using AutoMapper;
using AutoMapper.QueryableExtensions;
using CrawlScope.Application.Abstractions.Persistence;
using CrawlScope.Application.Common.Pagination;
using CrawlScope.Application.Modules.Crawling.DTOs;
using CrawlScope.Domain.Modules.Crawling.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CrawlScope.Application.Modules.Crawling.Queries.GetCrawlJobs
{
    public class GetCrawlJobsQueryHandler(IAppDbContext dbContext, IMapper mapper)
        : IRequestHandler<GetCrawlJobsQuery, PagedResult<CrawlJobListItemDto>>
    {
        public async Task<PagedResult<CrawlJobListItemDto>> Handle(GetCrawlJobsQuery request, CancellationToken cancellationToken)
        {
            var query = dbContext.CrawlJobs.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.Trim();
                query = query.Where(x => x.TargetUrl.Contains(search));
            }

            if (!string.IsNullOrWhiteSpace(request.Status)
                && Enum.TryParse<CrawlJobStatus>(request.Status, ignoreCase: true, out var status))
            {
                query = query.Where(x => x.Status == status);
            }

            var projectedQuery = query
                .OrderByDescending(cj => cj.CreatedAt)
                .ProjectTo<CrawlJobListItemDto>(mapper.ConfigurationProvider);

            return await PagedResult<CrawlJobListItemDto>.CreateAsync(
                projectedQuery,
                request.PageNumber,
                request.PageSize,
                cancellationToken);
        }
    }
}
