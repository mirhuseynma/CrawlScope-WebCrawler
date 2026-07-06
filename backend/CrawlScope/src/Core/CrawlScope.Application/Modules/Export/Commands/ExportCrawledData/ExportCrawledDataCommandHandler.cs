using System.Globalization;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using CrawlScope.Application.Abstractions.Export.Services;
using CrawlScope.Application.Abstractions.Persistence;
using CrawlScope.Application.Common.Exceptions;
using CrawlScope.Application.Modules.Export.DTOs;
using CrawlScope.Domain.Modules.Crawling.Enums;
using CrawlScope.Domain.Modules.Export.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CrawlScope.Application.Modules.Export.Commands.ExportCrawledData
{
    public class ExportCrawledDataCommandHandler(
        IAppDbContext context,
        IExportFileStorage exportFileStorage) : IRequestHandler<ExportCrawledDataCommand, ExportCrawledDataResultDto>
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

            var pages = await context.CrawledPages
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
                .ToListAsync(cancellationToken);

            var createdAt = DateTime.UtcNow;
            var extension = request.Format == ExportFormat.Csv ? "csv" : "json";
            var contentType = request.Format == ExportFormat.Csv
                ? "text/csv; charset=utf-8"
                : "application/json; charset=utf-8";
            var fileName = $"crawl-{request.CrawlJobId:N}-{createdAt:yyyyMMddHHmmss}.{extension}";
            var content = request.Format switch
            {
                ExportFormat.Csv => BuildCsv(pages),
                ExportFormat.Json => BuildJson(request.CrawlJobId, pages, createdAt),
                _ => throw new ArgumentException($"Unsupported export format: {request.Format}.")
            };

            var filePath = await exportFileStorage.SaveAsync(fileName, content, cancellationToken);
            var exportFile = new ExportFile
            {
                Id = Guid.NewGuid(),
                CrawlJobId = request.CrawlJobId,
                Format = request.Format,
                FileName = fileName,
                FilePath = filePath,
                CreatedAt = createdAt,
                CreatedByUserId = request.CreatedByUserId
            };

            await context.ExportFiles.AddAsync(exportFile, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            return new ExportCrawledDataResultDto
            {
                ExportFileId = exportFile.Id,
                FileName = fileName,
                ContentType = contentType,
                Content = content,
                CreatedAt = createdAt
            };
        }

        private static byte[] BuildJson(Guid crawlJobId, IReadOnlyCollection<ExportPageRow> pages, DateTime exportedAt)
        {
            var payload = new
            {
                crawlJobId,
                exportedAt,
                pageCount = pages.Count,
                pages
            };

            return JsonSerializer.SerializeToUtf8Bytes(payload, new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                WriteIndented = true
            });
        }

        private static byte[] BuildCsv(IReadOnlyCollection<ExportPageRow> pages)
        {
            var builder = new StringBuilder();
            builder.AppendLine("Url,Title,StatusCode,DepthLevel,CrawledAt,ResponseTimeMs,InternalLinksCount,ExternalLinksCount,Content");

            foreach (var page in pages)
            {
                var internalLinksCount = page.Links.Count(x => !x.IsExternal);
                var externalLinksCount = page.Links.Count(x => x.IsExternal);

                builder
                    .Append(EscapeCsv(page.Url)).Append(',')
                    .Append(EscapeCsv(page.Title)).Append(',')
                    .Append(page.StatusCode?.ToString(CultureInfo.InvariantCulture)).Append(',')
                    .Append(page.DepthLevel.ToString(CultureInfo.InvariantCulture)).Append(',')
                    .Append(EscapeCsv(page.CrawledAt.ToString("O", CultureInfo.InvariantCulture))).Append(',')
                    .Append(page.ResponseTimeMs?.ToString(CultureInfo.InvariantCulture)).Append(',')
                    .Append(internalLinksCount.ToString(CultureInfo.InvariantCulture)).Append(',')
                    .Append(externalLinksCount.ToString(CultureInfo.InvariantCulture)).Append(',')
                    .Append(EscapeCsv(page.Content))
                    .AppendLine();
            }

            return Encoding.UTF8.GetPreamble()
                .Concat(Encoding.UTF8.GetBytes(builder.ToString()))
                .ToArray();
        }

        private static string EscapeCsv(string? value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            var normalized = value.Replace("\r\n", "\n").Replace('\r', '\n');
            var mustQuote = normalized.Contains(',') || normalized.Contains('"') || normalized.Contains('\n');

            if (mustQuote)
            {
                normalized = $"\"{normalized.Replace("\"", "\"\"")}\"";
            }

            return normalized;
        }

        private class ExportPageRow
        {
            public string Url { get; set; } = null!;
            public string? Title { get; set; }
            public string? Content { get; set; }
            public int? StatusCode { get; set; }
            public int DepthLevel { get; set; }
            public DateTime CrawledAt { get; set; }
            public long? ResponseTimeMs { get; set; }
            public IReadOnlyCollection<ExportLinkRow> Links { get; set; } = [];
        }

        private class ExportLinkRow
        {
            public string SourceUrl { get; set; } = null!;
            public string TargetUrl { get; set; } = null!;
            public string? AnchorText { get; set; }
            public bool IsExternal { get; set; }
            public int DepthLevel { get; set; }
        }
    }
}
