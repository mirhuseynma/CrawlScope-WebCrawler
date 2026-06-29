using CrawlScope.Application.Modules.Crawling.DTOs;
using MediatR;

namespace CrawlScope.Application.Modules.Crawling.Queries.GetCrawlSchedules
{
    public record GetCrawlSchedulesQuery : IRequest<IEnumerable<CrawlScheduleListItemDto>>;
}
