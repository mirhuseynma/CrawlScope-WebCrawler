namespace CrawlScope.Application.Modules.Crawling.Queries.GetCrawlJobs
{
    public record GetCrawlJobsQuery(
        string? Search,
        string? Status,
        bool? ImportantOnly,
        int PageNumber,
        int PageSize,
        string RequestingUserId,
        bool IncludeAllUsers) : IRequest<PagedResult<CrawlJobListItemDto>>;
}
