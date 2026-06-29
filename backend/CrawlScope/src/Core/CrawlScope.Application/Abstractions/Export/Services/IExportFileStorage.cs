namespace CrawlScope.Application.Abstractions.Export.Services
{
    public interface IExportFileStorage
    {
        Task<string> SaveAsync(string fileName, byte[] content, CancellationToken cancellationToken = default);
    }
}
