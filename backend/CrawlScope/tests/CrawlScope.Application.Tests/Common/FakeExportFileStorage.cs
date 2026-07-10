using CrawlScope.Application.Abstractions.Export.Services;

namespace CrawlScope.Application.Tests.Common;

internal class FakeExportFileStorage : IExportFileStorage
{
    public List<string> DeletedPaths { get; } = [];

    public Task<string> SaveAsync(string fileName, byte[] content, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(fileName);
    }

    public Task<byte[]?> ReadAsync(string filePath, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<byte[]?>(null);
    }

    public Task DeleteAsync(string filePath, CancellationToken cancellationToken = default)
    {
        DeletedPaths.Add(filePath);
        return Task.CompletedTask;
    }
}
