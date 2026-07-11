namespace CrawlScope.Application.Modules.Export.Commands.ExportCrawledData
{
    public record ExportCrawledDataCommand(
        Guid CrawlJobId,
        ExportFormat Format,
        string CreatedByUserId,
        bool IncludeAllUsers) : IRequest<ExportCrawledDataResultDto>;
}
