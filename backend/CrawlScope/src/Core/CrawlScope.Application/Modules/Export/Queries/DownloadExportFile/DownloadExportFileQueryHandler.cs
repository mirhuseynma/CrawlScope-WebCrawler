using CrawlScope.Application.Abstractions.Export.Services;
using CrawlScope.Application.Abstractions.Persistence;
using CrawlScope.Application.Common.Exceptions;
using CrawlScope.Application.Modules.Export.DTOs;
using CrawlScope.Domain.Modules.Crawling.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CrawlScope.Application.Modules.Export.Queries.DownloadExportFile
{
    public class DownloadExportFileQueryHandler(
        IAppDbContext context,
        IExportFileStorage exportFileStorage) : IRequestHandler<DownloadExportFileQuery, ExportFileDownloadDto>
    {
        public async Task<ExportFileDownloadDto> Handle(DownloadExportFileQuery request, CancellationToken cancellationToken)
        {
            var exportFile = await context.ExportFiles
                .AsNoTracking()
                .Include(x => x.CrawlJob)
                .FirstOrDefaultAsync(
                    x => x.Id == request.Id
                        && (request.IncludeAllUsers || x.CreatedByUserId == request.RequestingUserId),
                    cancellationToken)
                ?? throw new NotFoundException("Export file not found.");

            var content = await exportFileStorage.ReadAsync(exportFile.FilePath, cancellationToken)
                ?? throw new NotFoundException("Export file content not found.");

            return new ExportFileDownloadDto
            {
                FileName = exportFile.FileName,
                ContentType = GetContentType(exportFile.Format),
                Content = content
            };
        }

        private static string GetContentType(ExportFormat format)
        {
            return format == ExportFormat.Csv
                ? "text/csv; charset=utf-8"
                : "application/json; charset=utf-8";
        }
    }
}
