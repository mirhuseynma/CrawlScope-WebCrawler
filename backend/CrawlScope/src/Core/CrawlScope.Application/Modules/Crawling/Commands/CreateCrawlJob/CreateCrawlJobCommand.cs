using CrawlScope.Application.Modules.Crawling.DTOs;
using MediatR;

namespace CrawlScope.Application.Modules.Crawling.Commands.CreateCrawlJob
{
    public record CreateCrawlJobCommand(CreateCrawlJobRequest Dto, string CreatedByUserId) : IRequest<Guid>
    {
    }
}
