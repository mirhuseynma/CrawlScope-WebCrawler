
namespace CrawlScope.Application.Modules.Crawling.Queries.AnalyzeUrl
{
    public record AnalyzeUrlQuery(string Url) : IRequest<AnalyzeUrlResultDto>;
}
