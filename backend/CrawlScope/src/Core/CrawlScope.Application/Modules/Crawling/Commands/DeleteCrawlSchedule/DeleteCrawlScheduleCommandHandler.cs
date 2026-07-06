using CrawlScope.Application.Abstractions.Persistence;
using CrawlScope.Application.Common.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CrawlScope.Application.Modules.Crawling.Commands.DeleteCrawlSchedule
{
    public class DeleteCrawlScheduleCommandHandler(IAppDbContext context)
        : IRequestHandler<DeleteCrawlScheduleCommand>
    {
        public async Task Handle(DeleteCrawlScheduleCommand request, CancellationToken cancellationToken)
        {
            var schedule = await context.CrawlSchedules
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
                ?? throw new NotFoundException($"Crawl schedule with ID {request.Id} not found.");

            context.CrawlSchedules.Remove(schedule);
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
