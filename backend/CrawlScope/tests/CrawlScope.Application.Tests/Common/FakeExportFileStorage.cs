using CrawlScope.Application.Abstractions.Export.Services;

namespace CrawlScope.Application.Tests.Common;

internal class FakeExportFileStorage : IExportFileStorage
{
    public List<string> DeletedPaths { get; } = [];
    public Dictionary<string, byte[]> Files { get; } = [];
    public string? LastSavedFileName { get; private set; }

    public Task<string> SaveAsync(string fileName, byte[] content, CancellationToken cancellationToken = default)
    {
        LastSavedFileName = fileName;
        var filePath = $"stored/{fileName}";
        Files[filePath] = content;
        return Task.FromResult(filePath);
    }

    public Task<byte[]?> ReadAsync(string filePath, CancellationToken cancellationToken = default)
    {
        Files.TryGetValue(filePath, out var content);
        return Task.FromResult<byte[]?>(content);
    }

    public Task DeleteAsync(string filePath, CancellationToken cancellationToken = default)
    {
        DeletedPaths.Add(filePath);
        Files.Remove(filePath);
        return Task.CompletedTask;
    }
}
