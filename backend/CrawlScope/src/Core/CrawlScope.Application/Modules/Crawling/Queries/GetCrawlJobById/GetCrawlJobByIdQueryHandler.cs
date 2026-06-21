using AutoMapper;
using AutoMapper.QueryableExtensions;
using CrawlScope.Application.Abstractions.Persistence;
using CrawlScope.Application.Modules.Crawling.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace CrawlScope.Application.Modules.Crawling.Queries.GetCrawlJobById
{
    public class GetCrawlJobByIdQueryHandler(IAppDbContext context, IMapper mapper) : IRequestHandler<GetCrawlJobByIdQuery, CrawlJobDetailsDto?>
    {
        public async Task<CrawlJobDetailsDto?> Handle(GetCrawlJobByIdQuery request, CancellationToken cancellationToken)
        {
            var crawljob = await context.CrawlJobs.AsNoTracking().Where(x => x.Id == request.Id).ProjectTo<CrawlJobDetailsDto>(mapper.ConfigurationProvider).FirstOrDefaultAsync(cancellationToken) ?? throw new InvalidOperationException("Crawl job not found");
            return crawljob;
        }
    }
}
