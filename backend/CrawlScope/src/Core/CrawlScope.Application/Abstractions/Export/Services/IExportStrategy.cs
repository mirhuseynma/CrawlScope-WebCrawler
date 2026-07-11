using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CrawlScope.Application.Modules.Export.DTOs;
using CrawlScope.Domain.Modules.Crawling.Enums;

namespace CrawlScope.Application.Abstractions.Export.Services
{
    public interface IExportStrategy
    {
        bool CanHandle(ExportFormat format);
        Task ExportAsync(Guid crawlJobId, IAsyncEnumerable<ExportPageRow> pages, Stream outputStream, CancellationToken cancellationToken = default);
        string GetContentType();
        string GetFileExtension();
    }
}
