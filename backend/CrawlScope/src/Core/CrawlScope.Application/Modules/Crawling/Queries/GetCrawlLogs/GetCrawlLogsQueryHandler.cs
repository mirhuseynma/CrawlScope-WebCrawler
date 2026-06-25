using CrawlScope.Application.Abstractions.Persistence;
using CrawlScope.Application.Modules.Crawling.DTOs;
using CrawlScope.Domain.Modules.Crawling.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CrawlScope.Application.Modules.Crawling.Queries.GetCrawlLogs
{
    public class GetCrawlLogsQueryHandler(IAppDbContext context)
        : IRequestHandler<GetCrawlLogsQuery, IEnumerable<CrawlLogDto>>
    {
        public async Task<IEnumerable<CrawlLogDto>> Handle(GetCrawlLogsQuery request, CancellationToken cancellationToken)
        {
            var query = context.CrawlLogs
                .AsNoTracking()
                .Where(x => x.CrawlJobId == request.CrawlJobId);

            if (!string.IsNullOrWhiteSpace(request.Level)
                && Enum.TryParse<CrawlLogLevel>(request.Level, ignoreCase: true, out var level))
            {
                query = query.Where(x => x.Level == level);
            }

            return await query
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new CrawlLogDto
                {
                    Id = x.Id,
                    Level = x.Level.ToString(),
                    Message = x.Message,
                    CreatedAt = x.CreatedAt
                })
                .ToListAsync(cancellationToken);
        }
    }
}
