using CrawlScope.Application.Common.Pagination;
using CrawlScope.Application.Modules.Crawling.DTOs;
using MediatR;

namespace CrawlScope.Application.Modules.Crawling.Queries.GetCrawledPages
{
    public record GetCrawledPagesQuery(
        Guid? CrawlJobId,
        string? Search,
        int? StatusCode,
        int? DepthLevel,
        int PageNumber,
        int PageSize,
        string RequestingUserId,
        bool IncludeAllUsers) : IRequest<PagedResult<CrawledPageListItemDto>>;
}
