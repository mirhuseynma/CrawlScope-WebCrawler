namespace CrawlScope.Application.Modules.Export.DTOs
{
    public class ExportFileListItemDto
    {
        public Guid Id { get; set; }
        public Guid CrawlJobId { get; set; }
        public string CrawlJobTargetUrl { get; set; } = null!;
        public string Format { get; set; } = null!;
        public string FileName { get; set; } = null!;
        public long FileSizeBytes { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
