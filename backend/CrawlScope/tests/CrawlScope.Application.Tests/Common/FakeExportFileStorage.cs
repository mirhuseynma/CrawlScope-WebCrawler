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

    public string GetFilePath(string fileName)
    {
        return fileName;
    }

    public Stream CreateFileStream(string fileName)
    {
        var filePath = GetFilePath(fileName);
        return new FakeStream(this, filePath);
    }

    private class FakeStream : MemoryStream
    {
        private readonly FakeExportFileStorage _storage;
        private readonly string _filePath;

        public FakeStream(FakeExportFileStorage storage, string filePath)
        {
            _storage = storage;
            _filePath = filePath;
        }

        protected override void Dispose(bool disposing)
        {
            _storage.Files[_filePath] = ToArray();
            base.Dispose(disposing);
        }
    }

    public Stream? OpenFileStream(string filePath)
    {
        if (Files.TryGetValue(filePath, out var content))
        {
            return new MemoryStream(content);
        }
        return null;
    }
}
