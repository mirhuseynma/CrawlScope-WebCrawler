
namespace CrawlScope.Application.Abstractions.Crawling.Services
{
    public interface IUrlAnalyzerService
    {
        Task<CrawlType> AnalyzeUrlAsync(string url, CancellationToken cancellationToken = default);
    }
}
