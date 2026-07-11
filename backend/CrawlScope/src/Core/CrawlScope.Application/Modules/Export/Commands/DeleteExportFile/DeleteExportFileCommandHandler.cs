namespace CrawlScope.Application.Modules.Export.Commands.DeleteExportFile
{
    public class DeleteExportFileCommandHandler(
        IAppDbContext context,
        IExportFileStorage exportFileStorage) : IRequestHandler<DeleteExportFileCommand>
    {
        public async Task Handle(DeleteExportFileCommand request, CancellationToken cancellationToken)
        {
            var exportFile = await context.ExportFiles
                .Include(x => x.CrawlJob)
                .FirstOrDefaultAsync(
                    x => x.Id == request.Id
                        && (request.IncludeAllUsers || x.CrawlJob.CreatedBy == request.RequestingUserId),
                    cancellationToken)
                ?? throw new NotFoundException($"Export file with ID {request.Id} not found.");

            var filePath = exportFile.FilePath;
            context.ExportFiles.Remove(exportFile);
            await context.SaveChangesAsync(cancellationToken);

            await exportFileStorage.DeleteAsync(filePath, cancellationToken);
        }
    }
}
