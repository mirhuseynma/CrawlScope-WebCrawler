using CrawlScope.Application.Common.Pagination;
using CrawlScope.Application.Modules.Crawling.DTOs;
using MediatR;

namespace CrawlScope.Application.Modules.Crawling.Queries.GetCrawlLogs
{
    public record GetCrawlLogsQuery(
        Guid CrawlJobId,
        string? Level,
        int PageNumber,
        int PageSize) : IRequest<PagedResult<CrawlLogDto>>;
}
