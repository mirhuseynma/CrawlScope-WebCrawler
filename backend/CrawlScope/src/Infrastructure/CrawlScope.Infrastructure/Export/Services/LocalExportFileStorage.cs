using CrawlScope.Application.Abstractions.Export.Services;

namespace CrawlScope.Infrastructure.Export.Services
{
    public class LocalExportFileStorage : IExportFileStorage
    {


        public Task DeleteAsync(string filePath, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                return Task.CompletedTask;
            }

            File.Delete(filePath);
            return Task.CompletedTask;
        }

        public string GetFilePath(string fileName)
        {
            var exportDirectory = Path.Combine(AppContext.BaseDirectory, "exports");
            Directory.CreateDirectory(exportDirectory);
            return Path.Combine(exportDirectory, fileName);
        }

        public Stream CreateFileStream(string fileName)
        {
            var filePath = GetFilePath(fileName);
            return new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true);
        }

        public Stream? OpenFileStream(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                return null;
            }

            return new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);
        }
    }
}
