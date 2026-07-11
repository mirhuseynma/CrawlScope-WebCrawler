
namespace CrawlScope.Application.Modules.Crawling.Commands.SetCrawlScheduleStatus
{
    public class SetCrawlScheduleStatusCommandHandler(IAppDbContext context)
        : IRequestHandler<SetCrawlScheduleStatusCommand>
    {
        public async Task Handle(SetCrawlScheduleStatusCommand request, CancellationToken cancellationToken)
        {
            var schedule = await context.CrawlSchedules
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
                ?? throw new NotFoundException($"Crawl schedule with ID {request.Id} not found.");

            schedule.IsEnabled = request.IsEnabled;

            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
