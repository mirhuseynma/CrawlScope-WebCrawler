using CrawlScope.Application.Abstractions.Persistence;
using CrawlScope.Application.Common.Pagination;
using CrawlScope.Application.Modules.Crawling.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CrawlScope.Application.Modules.Crawling.Queries.GetCrawledPages
{
    public class GetCrawledPagesQueryHandler(IAppDbContext context)
        : IRequestHandler<GetCrawledPagesQuery, PagedResult<CrawledPageListItemDto>>
    {
        public async Task<PagedResult<CrawledPageListItemDto>> Handle(GetCrawledPagesQuery request, CancellationToken cancellationToken)
        {
            var query = context.CrawledPages
                .AsNoTracking()
                .Where(x => x.CrawlJobId == request.CrawlJobId);

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.Trim();
                query = query.Where(x =>
                    x.Url.Contains(search)
                    || (x.Title != null && x.Title.Contains(search))
                    || (x.Content != null && x.Content.Contains(search)));
            }

            if (request.StatusCode.HasValue)
            {
                query = query.Where(x => x.StatusCode == request.StatusCode.Value);
            }

            if (request.DepthLevel.HasValue)
            {
                query = query.Where(x => x.DepthLevel == request.DepthLevel.Value);
            }

            var projectedQuery = query
                .OrderBy(x => x.DepthLevel)
                .ThenBy(x => x.CrawledAt)
                .Select(x => new CrawledPageListItemDto
                {
                    Id = x.Id,
                    Url = x.Url,
                    Title = x.Title,
                    ContentPreview = x.Content,
                    StatusCode = x.StatusCode,
                    DepthLevel = x.DepthLevel,
                    CrawledAt = x.CrawledAt,
                    ResponseTimeMs = x.ResponseTimeMs,
                    InternalLinksCount = x.Links.Count(link => !link.IsExternal),
                    ExternalLinksCount = x.Links.Count(link => link.IsExternal)
                });

            return await PagedResult<CrawledPageListItemDto>.CreateAsync(
                projectedQuery,
                request.PageNumber,
                request.PageSize,
                cancellationToken);
        }
    }
}
