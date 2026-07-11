namespace CrawlScope.Application.Modules.Crawling.Queries.GetCrawlSchedules
{
    public record GetCrawlSchedulesQuery(
        string? Search,
        bool? IsEnabled,
        int PageNumber,
        int PageSize) : IRequest<PagedResult<CrawlScheduleListItemDto>>;
}
