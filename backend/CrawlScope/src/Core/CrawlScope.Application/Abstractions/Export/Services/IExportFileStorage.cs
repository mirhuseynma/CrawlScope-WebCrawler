namespace CrawlScope.Application.Abstractions.Export.Services
{
    public interface IExportFileStorage
    {

        Task DeleteAsync(string filePath, CancellationToken cancellationToken = default);
        
        string GetFilePath(string fileName);
        Stream CreateFileStream(string fileName);
        Stream? OpenFileStream(string filePath);
    }
}
