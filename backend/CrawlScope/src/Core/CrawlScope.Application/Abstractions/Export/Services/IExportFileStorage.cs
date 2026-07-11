namespace CrawlScope.Application.Abstractions.Export.Services
{
    public interface IExportFileStorage
    {
        Task<string> SaveAsync(string fileName, byte[] content, CancellationToken cancellationToken = default);
        Task<byte[]?> ReadAsync(string filePath, CancellationToken cancellationToken = default);
        Task DeleteAsync(string filePath, CancellationToken cancellationToken = default);
        
        string GetFilePath(string fileName);
        Stream CreateFileStream(string fileName);
        Stream? OpenFileStream(string filePath);
    }
}
