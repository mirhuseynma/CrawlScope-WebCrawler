using CrawlScope.Application.Modules.Crawling.DTOs;
using MediatR;

namespace CrawlScope.Application.Modules.Crawling.Queries.AnalyzeUrl
{
    public record AnalyzeUrlQuery(string Url) : IRequest<AnalyzeUrlResultDto>;
}
