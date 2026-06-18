using CrawlScope.Application.Modules.Crawling.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrawlScope.Application.Modules.Crawling.Queries.GetCrawlJobs
{
    public record GetCrawlJobsQuery : IRequest<IEnumerable<CrawlJobListItemDto>>
    {
    }
}
