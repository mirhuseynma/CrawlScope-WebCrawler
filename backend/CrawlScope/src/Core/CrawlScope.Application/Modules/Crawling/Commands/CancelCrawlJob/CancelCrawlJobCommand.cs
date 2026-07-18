namespace CrawlScope.Application.Modules.Crawling.Commands.CancelCrawlJob
{
    public record CancelCrawlJobCommand(Guid Id, string RequestingUserId, bool IncludeAllUsers) : IRequest;
}
