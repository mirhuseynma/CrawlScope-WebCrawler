using CrawlScope.Application.Abstractions.Crawling.Services;
using CrawlScope.Application.Modules.Crawling.DTOs;
using MediatR;

namespace CrawlScope.Application.Modules.Crawling.Queries.AnalyzeUrl
{
    public class AnalyzeUrlQueryHandler(IUrlAnalyzerService analyzerService) : IRequestHandler<AnalyzeUrlQuery, AnalyzeUrlResultDto>
    {
        public async Task<AnalyzeUrlResultDto> Handle(AnalyzeUrlQuery request, CancellationToken cancellationToken)
        {
            var recommendedType = await analyzerService.AnalyzeUrlAsync(request.Url, cancellationToken);
            
            return new AnalyzeUrlResultDto
            {
                RecommendedType = recommendedType,
                RecommendationReason = recommendedType == Domain.Modules.Crawling.Enums.CrawlType.Dynamic 
                    ? "Bot protection or Single Page Application (SPA) detected." 
                    : "Standard HTML site detected."
            };
        }
    }
}
