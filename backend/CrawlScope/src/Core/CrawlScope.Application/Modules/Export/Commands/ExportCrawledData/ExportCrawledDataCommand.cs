using CrawlScope.Application.Modules.Export.DTOs;
using CrawlScope.Domain.Modules.Crawling.Enums;
using MediatR;

namespace CrawlScope.Application.Modules.Export.Commands.ExportCrawledData
{
    public record ExportCrawledDataCommand(
        Guid CrawlJobId,
        ExportFormat Format,
        string CreatedByUserId,
        bool IncludeAllUsers) : IRequest<ExportCrawledDataResultDto>;
}
