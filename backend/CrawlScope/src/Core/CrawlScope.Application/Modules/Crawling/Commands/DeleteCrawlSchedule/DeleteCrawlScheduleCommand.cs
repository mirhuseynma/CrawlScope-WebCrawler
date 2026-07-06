using MediatR;

namespace CrawlScope.Application.Modules.Crawling.Commands.DeleteCrawlSchedule
{
    public record DeleteCrawlScheduleCommand(Guid Id) : IRequest;
}
