using CrawlScope.Application.Common.Pagination;
using CrawlScope.Application.Modules.Export.DTOs;
using MediatR;

namespace CrawlScope.Application.Modules.Export.Queries.GetExportFiles
{
    public record GetExportFilesQuery(
        string? Search,
        string? Format,
        int PageNumber,
        int PageSize,
        string RequestingUserId,
        bool IncludeAllUsers) : IRequest<PagedResult<ExportFileListItemDto>>;
}
