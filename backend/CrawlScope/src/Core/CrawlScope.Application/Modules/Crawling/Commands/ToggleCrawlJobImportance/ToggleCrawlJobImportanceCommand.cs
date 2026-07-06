using MediatR;

namespace CrawlScope.Application.Modules.Crawling.Commands.ToggleCrawlJobImportance
{
    public record ToggleCrawlJobImportanceCommand(Guid Id) : IRequest<bool>;
}
