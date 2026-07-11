
namespace CrawlScope.Application.Modules.Crawling.Commands.CreateCrawlSchedule
{
    public record CreateCrawlScheduleCommand(
        CreateCrawlScheduleRequestDto Dto,
        string CreatedByUserId) : IRequest<Guid>;
}
