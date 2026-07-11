using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CrawlScope.Application.Abstractions.Export.Services;
using CrawlScope.Application.Modules.Export.DTOs;
using CrawlScope.Domain.Modules.Crawling.Enums;

namespace CrawlScope.Infrastructure.Export.Services
{
    public class CsvExportStrategy : IExportStrategy
    {
        public bool CanHandle(ExportFormat format) => format == ExportFormat.Csv;

        public string GetContentType() => "text/csv; charset=utf-8";

        public string GetFileExtension() => "csv";

        public async Task ExportAsync(Guid crawlJobId, IAsyncEnumerable<ExportPageRow> pages, Stream outputStream, CancellationToken cancellationToken = default)
        {
            using var writer = new StreamWriter(outputStream, new UTF8Encoding(true), 4096, leaveOpen: true);
            await writer.WriteLineAsync("Url,Title,StatusCode,DepthLevel,CrawledAt,ResponseTimeMs,InternalLinksCount,ExternalLinksCount,Content".AsMemory(), cancellationToken);

            await foreach (var page in pages.WithCancellation(cancellationToken))
            {
                var internalLinksCount = page.Links.Count(x => !x.IsExternal);
                var externalLinksCount = page.Links.Count(x => x.IsExternal);

                var builder = new StringBuilder();
                builder.Append(EscapeCsv(page.Url)).Append(',')
                       .Append(EscapeCsv(page.Title)).Append(',')
                       .Append(page.StatusCode?.ToString(CultureInfo.InvariantCulture)).Append(',')
                       .Append(page.DepthLevel.ToString(CultureInfo.InvariantCulture)).Append(',')
                       .Append(EscapeCsv(page.CrawledAt.ToString("O", CultureInfo.InvariantCulture))).Append(',')
                       .Append(page.ResponseTimeMs?.ToString(CultureInfo.InvariantCulture)).Append(',')
                       .Append(internalLinksCount.ToString(CultureInfo.InvariantCulture)).Append(',')
                       .Append(externalLinksCount.ToString(CultureInfo.InvariantCulture)).Append(',')
                       .Append(EscapeCsv(page.Content));

                await writer.WriteLineAsync(builder.ToString().AsMemory(), cancellationToken);
            }
            await writer.FlushAsync();
        }

        private static string EscapeCsv(string? value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;

            var normalized = value.Replace("\r\n", "\n").Replace('\r', '\n');
            var mustQuote = normalized.Contains(',') || normalized.Contains('"') || normalized.Contains('\n');

            if (mustQuote)
            {
                normalized = $"\"{normalized.Replace("\"", "\"\"")}\"";
            }

            return normalized;
        }
    }
}
