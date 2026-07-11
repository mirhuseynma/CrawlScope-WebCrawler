using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CrawlScope.Application.Abstractions.Export.Services;
using CrawlScope.Application.Modules.Export.DTOs;
using CrawlScope.Domain.Modules.Crawling.Enums;

namespace CrawlScope.Application.Tests.Common
{
    public class FakeExportStrategy(ExportFormat format, string content) : IExportStrategy
    {
        public bool CanHandle(ExportFormat f) => f == format;

        public async Task ExportAsync(Guid crawlJobId, IAsyncEnumerable<ExportPageRow> pages, Stream outputStream, CancellationToken cancellationToken = default)
        {
            var bytes = Encoding.UTF8.GetBytes(content);
            await outputStream.WriteAsync(bytes, cancellationToken);
        }

        public string GetContentType() => format == ExportFormat.Json ? "application/json; charset=utf-8" : "text/csv; charset=utf-8";

        public string GetFileExtension() => format.ToString().ToLower();
    }
}
