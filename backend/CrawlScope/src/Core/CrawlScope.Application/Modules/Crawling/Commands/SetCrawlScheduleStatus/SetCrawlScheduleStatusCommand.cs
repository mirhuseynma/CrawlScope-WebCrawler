
namespace CrawlScope.Application.Modules.Crawling.Commands.SetCrawlScheduleStatus
{
    public record SetCrawlScheduleStatusCommand(Guid Id, bool IsEnabled) : IRequest;
}
