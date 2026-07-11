namespace CrawlScope.Application.Modules.Crawling.Queries.GetBrokenLinks
{
    public record GetBrokenLinksQuery(
        Guid CrawlJobId,
        string? Search,
        int? StatusCode,
        bool? ExternalOnly,
        int PageNumber,
        int PageSize,
        string RequestingUserId,
        bool IncludeAllUsers) : IRequest<PagedResult<BrokenLinkListItemDto>>;
}
