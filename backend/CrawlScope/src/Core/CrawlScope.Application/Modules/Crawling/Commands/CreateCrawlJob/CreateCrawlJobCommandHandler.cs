
namespace CrawlScope.Application.Modules.Crawling.Commands.CreateCrawlJob
{
    public class CreateCrawlJobCommandHandler(IMapper mapper, IAppDbContext dbContext) : IRequestHandler<CreateCrawlJobCommand, Guid>
    {
        public async Task<Guid> Handle(CreateCrawlJobCommand request, CancellationToken cancellationToken)
        {
            var crawlJob = mapper.Map<CrawlJob>(request.Dto);
            crawlJob.Id = Guid.NewGuid();
            crawlJob.CreatedBy = request.CreatedByUserId;
            crawlJob.CreatedAt = DateTime.UtcNow;
            crawlJob.Status = CrawlJobStatus.Pending;
            await dbContext.CrawlJobs.AddAsync(crawlJob, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            return crawlJob.Id;
        }
    }
}
