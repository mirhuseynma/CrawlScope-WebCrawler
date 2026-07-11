namespace CrawlScope.Application.Modules.Crawling.Queries.GetCrawledPages
{
    public class GetCrawledPagesQueryHandler(IAppDbContext context)
        : IRequestHandler<GetCrawledPagesQuery, PagedResult<CrawledPageListItemDto>>
    {
        public async Task<PagedResult<CrawledPageListItemDto>> Handle(GetCrawledPagesQuery request, CancellationToken cancellationToken)
        {
            var query = context.CrawledPages
                .AsNoTracking()
                .WhereIf(!request.IncludeAllUsers, x => x.CrawlJob.CreatedBy == request.RequestingUserId)
                .WhereIf(request.CrawlJobId.HasValue, x => x.CrawlJobId == request.CrawlJobId!.Value);

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.Trim();
                query = query.Where(x =>
                    x.Url.Contains(search)
                    || (x.Title != null && x.Title.Contains(search))
                    || (x.Content != null && x.Content.Contains(search)));
            }

            query = query
                .WhereIf(request.StatusCode.HasValue, x => x.StatusCode == request.StatusCode!.Value)
                .WhereIf(request.DepthLevel.HasValue, x => x.DepthLevel == request.DepthLevel!.Value);

            var projectedQuery = query
                .OrderBy(x => x.DepthLevel)
                .ThenBy(x => x.CrawledAt)
                .Select(x => new CrawledPageListItemDto
                {
                    Id = x.Id,
                    CrawlJobId = x.CrawlJobId,
                    CrawlJobTargetUrl = x.CrawlJob.TargetUrl,
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
