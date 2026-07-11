

namespace CrawlScope.Application.Modules.Crawling.Commands.CreateCrawlSchedule
{
    public class CreateCrawlScheduleCommandHandler(IAppDbContext context)
        : IRequestHandler<CreateCrawlScheduleCommand, Guid>
    {
        public async Task<Guid> Handle(CreateCrawlScheduleCommand request, CancellationToken cancellationToken)
        {
            var schedule = new CrawlSchedule
            {
                Id = Guid.NewGuid(),
                TargetUrl = request.Dto.TargetUrl,
                MaxDepth = request.Dto.MaxDepth,
                MaxPages = request.Dto.MaxPages,
                StayWithinDomain = request.Dto.StayWithinDomain,
                IntervalMinutes = request.Dto.IntervalMinutes,
                IsEnabled = true,
                CreatedAt = DateTime.UtcNow,
                NextRunAt = DateTime.UtcNow,
                CreatedBy = request.CreatedByUserId
            };

            await context.CrawlSchedules.AddAsync(schedule, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            return schedule.Id;
        }
    }
}
