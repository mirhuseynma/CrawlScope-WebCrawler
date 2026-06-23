using CrawlScope.Application.Abstractions.Persistence;
using CrawlScope.Domain.Modules.Crawling.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace CrawlScope.Application.Modules.Crawling.Commands.StartCrawlJob
{
    public class StartCrawlJobCommandHandler(IAppDbContext context) : IRequestHandler<StartCrawlJobCommand>
    {
        public async Task Handle (StartCrawlJobCommand request, CancellationToken cancellationToken)
        {
            var crawlJob = await context.CrawlJobs.FirstOrDefaultAsync(x  => x.Id == request.Id, cancellationToken);

            if (crawlJob is null)
            {
                throw new InvalidOperationException($"Crawl job with ID {request.Id} not found.");
            }

            if (crawlJob.Status != CrawlJobStatus.Pending)
            {
                throw new InvalidOperationException($"Crawl job with ID {request.Id} is not in a pending state.");
            }

            crawlJob.Status = CrawlJobStatus.InProgress;
            crawlJob.StartedAt = DateTime.UtcNow;

            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
