namespace CrawlScope.Application.Modules.Crawling.Queries.GetBrokenLinks
{
    public class GetBrokenLinksQueryHandler(IAppDbContext context)
        : IRequestHandler<GetBrokenLinksQuery, PagedResult<BrokenLinkListItemDto>>
    {
        public async Task<PagedResult<BrokenLinkListItemDto>> Handle(GetBrokenLinksQuery request, CancellationToken cancellationToken)
        {
            var query = context.CrawlQueueItems
                .AsNoTracking()
                .Where(x => x.CrawlJobId == request.CrawlJobId)
                .Where(x => x.Status == CrawlQueueStatus.Failed)
                .WhereIf(!request.IncludeAllUsers, x => x.CrawlJob.CreatedBy == request.RequestingUserId);

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.Trim();
                query = query.Where(x =>
                    x.Url.Contains(search)
                    || (x.DiscoveredFromUrl != null && x.DiscoveredFromUrl.Contains(search))
                    || (x.AnchorText != null && x.AnchorText.Contains(search)));
            }

            query = query
                .WhereIf(request.StatusCode.HasValue, x => x.StatusCode == request.StatusCode!.Value)
                .WhereIf(request.ExternalOnly.HasValue, x => x.IsExternal == request.ExternalOnly!.Value);

            var projectedQuery = query
                .OrderByDescending(x => x.ProcessedAt ?? x.CreatedAt)
                .Select(x => new BrokenLinkListItemDto
                {
                    Id = x.Id,
                    CrawlJobId = x.CrawlJobId,
                    SourceUrl = x.DiscoveredFromUrl ?? x.CrawlJob.TargetUrl,
                    TargetUrl = x.Url,
                    AnchorText = x.AnchorText,
                    IsExternal = x.IsExternal,
                    DepthLevel = x.DepthLevel,
                    StatusCode = x.StatusCode,
                    ResponseTimeMs = x.ResponseTimeMs,
                    ErrorMessage = x.ErrorMessage,
                    DetectedAt = x.ProcessedAt ?? x.CreatedAt
                });

            return await PagedResult<BrokenLinkListItemDto>.CreateAsync(
                projectedQuery,
                request.PageNumber,
                request.PageSize,
                cancellationToken);
        }
    }
}
