using MediatR;

namespace CrawlScope.Application.Modules.Export.Commands.DeleteExportFile
{
    public record DeleteExportFileCommand(
        Guid Id,
        string RequestingUserId,
        bool IncludeAllUsers) : IRequest;
}
