
namespace CrawlScope.Application.Modules.Crawling.Commands.DeleteCrawlJob
{
    public class DeleteCrawlJobCommandHandler(
        IAppDbContext context,
        IExportFileStorage exportFileStorage) : IRequestHandler<DeleteCrawlJobCommand>
    {
        public async Task Handle(DeleteCrawlJobCommand request, CancellationToken cancellationToken)
        {
            var crawlJob = await context.CrawlJobs
                .Include(x => x.ExportFiles)
                .FirstOrDefaultAsync(
                    x => x.Id == request.Id
                        && (request.IncludeAllUsers || x.CreatedBy == request.RequestingUserId),
                    cancellationToken)
                ?? throw new NotFoundException($"Crawl job with ID {request.Id} not found.");

            if (crawlJob.Status == CrawlJobStatus.InProgress)
            {
                throw new InvalidOperationException("In-progress crawl jobs cannot be deleted.");
            }

            var exportFilePaths = crawlJob.ExportFiles
                .Select(x => x.FilePath)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToArray();

            context.CrawlJobs.Remove(crawlJob);
            await context.SaveChangesAsync(cancellationToken);

            foreach (var filePath in exportFilePaths)
            {
                await exportFileStorage.DeleteAsync(filePath, cancellationToken);
            }
        }
    }
}
