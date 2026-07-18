
namespace CrawlScope.Domain.Modules.Export.Models
{
    public class ExportFile : BaseEntity
    {
        public Guid CrawlJobId { get; set; }
        public CrawlJob CrawlJob { get; set; } = null!;
        public ExportFormat Format { get; set; }
        public string FileName { get; set; } = null!;
        public string FilePath { get; set; } = null!;
        public long FileSizeBytes { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedByUserId { get; set; } = null!;
    }
}
