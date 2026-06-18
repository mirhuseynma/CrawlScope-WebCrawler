using AutoMapper;
using AutoMapper.QueryableExtensions;
using CrawlScope.Application.Abstractions.Persistence;
using CrawlScope.Application.Modules.Crawling.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrawlScope.Application.Modules.Crawling.Queries.GetCrawlJobs
{
    public class GetCrawlJobsQueryHandler(IAppDbContext dbContext, IMapper mapper) : IRequestHandler<GetCrawlJobsQuery, IEnumerable<CrawlJobListItemDto>>
    {
        public async Task<IEnumerable<CrawlJobListItemDto>> Handle(GetCrawlJobsQuery request, CancellationToken cancellationToken)
        {
            var crawlJobs = await dbContext.CrawlJobs.AsNoTracking().OrderByDescending(cj => cj.CreatedAt).ProjectTo<CrawlJobListItemDto>(mapper.ConfigurationProvider).ToListAsync(cancellationToken);
            return crawlJobs;
        }
    }
}
