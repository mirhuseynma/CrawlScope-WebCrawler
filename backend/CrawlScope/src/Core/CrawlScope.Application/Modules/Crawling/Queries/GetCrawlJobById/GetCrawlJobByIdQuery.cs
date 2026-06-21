using CrawlScope.Application.Modules.Crawling.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace CrawlScope.Application.Modules.Crawling.Queries.GetCrawlJobById
{
    public record GetCrawlJobByIdQuery(Guid Id) : IRequest<CrawlJobDetailsDto?>
    {
    }
}
