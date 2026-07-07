using CrawlScope.Application.Abstractions.Export.Services;

namespace CrawlScope.Infrastructure.Export.Services
{
    public class LocalExportFileStorage : IExportFileStorage
    {
        public async Task<string> SaveAsync(string fileName, byte[] content, CancellationToken cancellationToken = default)
        {
            var exportDirectory = Path.Combine(AppContext.BaseDirectory, "exports");
            Directory.CreateDirectory(exportDirectory);

            var filePath = Path.Combine(exportDirectory, fileName);
            await File.WriteAllBytesAsync(filePath, content, cancellationToken);

            return filePath;
        }

        public async Task<byte[]?> ReadAsync(string filePath, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                return null;
            }

            return await File.ReadAllBytesAsync(filePath, cancellationToken);
        }

        public Task DeleteAsync(string filePath, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                return Task.CompletedTask;
            }

            File.Delete(filePath);
            return Task.CompletedTask;
        }
    }
}
