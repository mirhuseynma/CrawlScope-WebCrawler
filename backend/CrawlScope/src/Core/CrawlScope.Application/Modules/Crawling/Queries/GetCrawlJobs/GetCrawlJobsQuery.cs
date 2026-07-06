using CrawlScope.Application.Common.Pagination;
using CrawlScope.Application.Modules.Crawling.DTOs;
using MediatR;

namespace CrawlScope.Application.Modules.Crawling.Queries.GetCrawlJobs
{
    public record GetCrawlJobsQuery(
        string? Search,
        string? Status,
        int PageNumber,
        int PageSize,
        string RequestingUserId,
        bool IncludeAllUsers) : IRequest<PagedResult<CrawlJobListItemDto>>;
}
