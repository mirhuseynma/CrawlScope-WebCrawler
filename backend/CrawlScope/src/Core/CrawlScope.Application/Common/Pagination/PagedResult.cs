using Microsoft.EntityFrameworkCore;

namespace CrawlScope.Application.Common.Pagination
{
    public class PagedResult<T>
    {
        public IReadOnlyList<T> Items { get; init; } = [];
        public int PageNumber { get; init; }
        public int PageSize { get; init; }
        public int TotalCount { get; init; }
        public int TotalPages { get; init; }
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;

        public static async Task<PagedResult<T>> CreateAsync(
            IQueryable<T> query,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken)
        {
            var safePageNumber = Math.Max(pageNumber, 1);
            var safePageSize = Math.Clamp(pageSize, 1, 100);
            var totalCount = await query.CountAsync(cancellationToken);
            var totalPages = totalCount == 0
                ? 0
                : (int)Math.Ceiling(totalCount / (double)safePageSize);

            var items = await query
                .Skip((safePageNumber - 1) * safePageSize)
                .Take(safePageSize)
                .ToListAsync(cancellationToken);

            return new PagedResult<T>
            {
                Items = items,
                PageNumber = safePageNumber,
                PageSize = safePageSize,
                TotalCount = totalCount,
                TotalPages = totalPages
            };
        }
    }
}
