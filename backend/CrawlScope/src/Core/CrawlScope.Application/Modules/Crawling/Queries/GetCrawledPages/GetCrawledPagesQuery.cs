using CrawlScope.Application.Modules.Crawling.DTOs;
using MediatR;

namespace CrawlScope.Application.Modules.Crawling.Queries.GetCrawledPages
{
    public record GetCrawledPagesQuery(
        Guid CrawlJobId,
        string? Search,
        int? StatusCode,
        int? DepthLevel) : IRequest<IEnumerable<CrawledPageListItemDto>>;
}
