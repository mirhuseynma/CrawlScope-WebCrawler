using AutoMapper.QueryableExtensions;

namespace CrawlScope.Application.Modules.Crawling.Queries.GetCrawlJobById
{
    public class GetCrawlJobByIdQueryHandler(IAppDbContext context, IMapper mapper)
        : IRequestHandler<GetCrawlJobByIdQuery, CrawlJobDetailsDto?>
    {
        public async Task<CrawlJobDetailsDto?> Handle(GetCrawlJobByIdQuery request, CancellationToken cancellationToken)
        {
            return await context.CrawlJobs
                .AsNoTracking()
                .Where(x => x.Id == request.Id)
                .Where(x => request.IncludeAllUsers || x.CreatedBy == request.RequestingUserId)
                .ProjectTo<CrawlJobDetailsDto>(mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new NotFoundException("Crawl job not found.");
        }
    }
}
