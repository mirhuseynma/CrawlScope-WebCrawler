using CrawlScope.Application.Abstractions.Persistence;
using CrawlScope.Application.Common.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CrawlScope.Application.Modules.Crawling.Commands.ToggleCrawlJobImportance
{
    public class ToggleCrawlJobImportanceCommandHandler(IAppDbContext context)
        : IRequestHandler<ToggleCrawlJobImportanceCommand, bool>
    {
        public async Task<bool> Handle(ToggleCrawlJobImportanceCommand request, CancellationToken cancellationToken)
        {
            var crawlJob = await context.CrawlJobs
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
                ?? throw new NotFoundException("Crawl job not found.");

            crawlJob.IsImportant = !crawlJob.IsImportant;
            await context.SaveChangesAsync(cancellationToken);

            return crawlJob.IsImportant;
        }
    }
}
