using MediatR;

namespace CrawlScope.Application.Modules.Crawling.Commands.DeleteCrawlJob
{
    public record DeleteCrawlJobCommand(
        Guid Id,
        string RequestingUserId,
        bool IncludeAllUsers) : IRequest;
}
