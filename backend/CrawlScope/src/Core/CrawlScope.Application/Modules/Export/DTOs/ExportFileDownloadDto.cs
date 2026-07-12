namespace CrawlScope.Application.Modules.Export.DTOs
{
    public class ExportFileDownloadDto
    {
        public string FileName { get; set; } = null!;
        public string ContentType { get; set; } = null!;
        public Stream ContentStream { get; set; } = null!;
    }
}
