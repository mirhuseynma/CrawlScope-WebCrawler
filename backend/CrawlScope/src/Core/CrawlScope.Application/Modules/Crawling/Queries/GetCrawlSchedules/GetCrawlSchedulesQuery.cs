using CrawlScope.Application.Common.Pagination;
using CrawlScope.Application.Modules.Crawling.DTOs;
using MediatR;

namespace CrawlScope.Application.Modules.Crawling.Queries.GetCrawlSchedules
{
    public record GetCrawlSchedulesQuery(
        string? Search,
        bool? IsEnabled,
        int PageNumber,
        int PageSize) : IRequest<PagedResult<CrawlScheduleListItemDto>>;
}
