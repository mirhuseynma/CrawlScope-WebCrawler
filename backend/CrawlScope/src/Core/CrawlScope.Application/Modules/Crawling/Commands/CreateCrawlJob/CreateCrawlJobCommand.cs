

namespace CrawlScope.Application.Modules.Crawling.Commands.CreateCrawlJob
{
    public record CreateCrawlJobCommand(CreateCrawlJobRequestDto Dto, string CreatedByUserId) : IRequest<Guid>
    {
    }
}
