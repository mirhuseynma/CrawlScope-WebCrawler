using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CrawlScope.Application.Abstractions.Export.Services;
using CrawlScope.Application.Abstractions.Persistence;
using CrawlScope.Application.Common.Exceptions;
using CrawlScope.Application.Modules.Export.DTOs;
using CrawlScope.Domain.Modules.Export.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CrawlScope.Application.Modules.Export.Commands.ExportCrawledData
{
    public class ExportCrawledDataCommandHandler(
        IAppDbContext context,
        IExportFileStorage exportFileStorage,
        IEnumerable<IExportStrategy> exportStrategies) : IRequestHandler<ExportCrawledDataCommand, ExportCrawledDataResultDto>
    {
        public async Task<ExportCrawledDataResultDto> Handle(ExportCrawledDataCommand request, CancellationToken cancellationToken)
        {
            var crawlJobExists = await context.CrawlJobs
                .AsNoTracking()
                .AnyAsync(
                    x => x.Id == request.CrawlJobId
                        && (request.IncludeAllUsers || x.CreatedBy == request.CreatedByUserId),
                    cancellationToken);

            if (!crawlJobExists)
            {
                throw new NotFoundException($"Crawl job with ID {request.CrawlJobId} not found.");
            }

            var strategy = exportStrategies.FirstOrDefault(s => s.CanHandle(request.Format))
                ?? throw new ArgumentException($"Unsupported export format: {request.Format}.");

            var pagesAsyncEnum = context.CrawledPages
                .AsNoTracking()
                .Where(x => x.CrawlJobId == request.CrawlJobId)
                .OrderBy(x => x.DepthLevel)
                .ThenBy(x => x.CrawledAt)
                .Select(x => new ExportPageRow
                {
                    Url = x.Url,
                    Title = x.Title,
                    Content = x.Content,
                    StatusCode = x.StatusCode,
                    DepthLevel = x.DepthLevel,
                    CrawledAt = x.CrawledAt,
                    ResponseTimeMs = x.ResponseTimeMs,
                    Links = x.Links
                        .OrderBy(link => link.IsExternal)
                        .ThenBy(link => link.TargetUrl)
                        .Select(link => new ExportLinkRow
                        {
                            SourceUrl = link.SourceUrl,
                            TargetUrl = link.TargetUrl,
                            AnchorText = link.AnchorText,
                            IsExternal = link.IsExternal,
                            DepthLevel = link.DepthLevel
                        })
                        .ToList()
                })
                .AsAsyncEnumerable();

            var createdAt = DateTime.UtcNow;
            var fileName = $"crawl-{request.CrawlJobId:N}-{createdAt:yyyyMMddHHmmss}.{strategy.GetFileExtension()}";
            var filePath = exportFileStorage.GetFilePath(fileName);

            long fileSizeBytes;
            await using (var fileStream = exportFileStorage.CreateFileStream(fileName))
            await using (var countingStream = new CrawlScope.Application.Common.Streams.CountingStream(fileStream))
            {
                await strategy.ExportAsync(request.CrawlJobId, pagesAsyncEnum, countingStream, cancellationToken);
                fileSizeBytes = countingStream.BytesWritten;
            }

            var exportFile = new ExportFile
            {
                Id = Guid.NewGuid(),
                CrawlJobId = request.CrawlJobId,
                Format = request.Format,
                FileName = fileName,
                FilePath = filePath,
                FileSizeBytes = fileSizeBytes,
                CreatedAt = createdAt,
                CreatedByUserId = request.CreatedByUserId
            };

            await context.ExportFiles.AddAsync(exportFile, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            return new ExportCrawledDataResultDto
            {
                ExportFileId = exportFile.Id,
                FileName = fileName,
                ContentType = strategy.GetContentType(),
                FilePath = filePath,
                CreatedAt = createdAt
            };
        }
    }
}
