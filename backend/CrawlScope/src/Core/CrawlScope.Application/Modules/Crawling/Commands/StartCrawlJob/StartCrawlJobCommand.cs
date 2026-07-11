

namespace CrawlScope.Application.Modules.Crawling.Commands.StartCrawlJob
{
    public record StartCrawlJobCommand(
        Guid Id,
        string RequestingUserId,
        bool IncludeAllUsers) : IRequest
    {
    }
}
