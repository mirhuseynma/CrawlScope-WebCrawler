namespace CrawlScope.Application.Modules.Export.DTOs
{
    public class ExportCrawledDataResultDto
    {
        public Guid ExportFileId { get; set; }
        public string FileName { get; set; } = null!;
        public string ContentType { get; set; } = null!;

        public string FilePath { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }
}
