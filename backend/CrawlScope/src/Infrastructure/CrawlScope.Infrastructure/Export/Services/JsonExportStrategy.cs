
namespace CrawlScope.Infrastructure.Export.Services
{
    public class JsonExportStrategy : IExportStrategy
    {
        public bool CanHandle(ExportFormat format) => format == ExportFormat.Json;

        public string GetContentType() => "application/json; charset=utf-8";

        public string GetFileExtension() => "json";

        public async Task ExportAsync(Guid crawlJobId, IAsyncEnumerable<ExportPageRow> pages, Stream outputStream, CancellationToken cancellationToken = default)
        {
            var options = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                WriteIndented = true
            };

            var writerOptions = new JsonWriterOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                Indented = true
            };

            await using var writer = new Utf8JsonWriter(outputStream, writerOptions);
            writer.WriteStartObject();
            writer.WriteString("crawlJobId", crawlJobId.ToString());
            writer.WriteString("exportedAt", DateTime.UtcNow.ToString("O"));
            
            writer.WritePropertyName("pages");
            writer.WriteStartArray();
            
            int count = 0;
            await foreach (var page in pages.WithCancellation(cancellationToken))
            {
                JsonSerializer.Serialize(writer, page, options);
                count++;
            }
            
            writer.WriteEndArray();
            writer.WriteNumber("pageCount", count);
            writer.WriteEndObject();
            
            await writer.FlushAsync();
        }
    }
}
