namespace CrawlScope.Application.Modules.Crawling.DTOs
{
    public class AnalyzeUrlResultDto
    {
        public CrawlType RecommendedType { get; set; }
        public string RecommendationReason { get; set; } = string.Empty;
    }
}
