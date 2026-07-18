
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
