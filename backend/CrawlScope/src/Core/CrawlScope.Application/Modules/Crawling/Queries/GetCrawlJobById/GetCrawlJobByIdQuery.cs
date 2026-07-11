namespace CrawlScope.Application.Modules.Crawling.Queries.GetCrawlJobById
{
    public record GetCrawlJobByIdQuery(
        Guid Id,
        string RequestingUserId,
        bool IncludeAllUsers) : IRequest<CrawlJobDetailsDto?>
    {
    }
}
