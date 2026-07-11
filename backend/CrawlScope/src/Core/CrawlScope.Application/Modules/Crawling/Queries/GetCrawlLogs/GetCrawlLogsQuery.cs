namespace CrawlScope.Application.Modules.Crawling.Queries.GetCrawlLogs
{
    public record GetCrawlLogsQuery(
        Guid CrawlJobId,
        string? Level,
        int PageNumber,
        int PageSize,
        string RequestingUserId,
        bool IncludeAllUsers) : IRequest<PagedResult<CrawlLogDto>>;
}
