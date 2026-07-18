
namespace CrawlScope.Application.Modules.Export.DTOs
{
    public class ExportLinkRow
    {
        public string SourceUrl { get; set; } = null!;
        public string TargetUrl { get; set; } = null!;
        public string? AnchorText { get; set; }
        public bool IsExternal { get; set; }
        public int DepthLevel { get; set; }
    }
}
