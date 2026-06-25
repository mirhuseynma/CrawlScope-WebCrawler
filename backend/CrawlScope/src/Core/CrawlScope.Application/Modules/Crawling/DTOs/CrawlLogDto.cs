namespace CrawlScope.Application.Modules.Crawling.DTOs
{
    public class CrawlLogDto
    {
        public Guid Id { get; set; }
        public string Level { get; set; } = null!;
        public string Message { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }
}
