namespace CrawlScope.Application.Modules.Export.Queries.DownloadExportFile
{
    public record DownloadExportFileQuery(
        Guid Id,
        string RequestingUserId,
        bool IncludeAllUsers) : IRequest<ExportFileDownloadDto>;
}
