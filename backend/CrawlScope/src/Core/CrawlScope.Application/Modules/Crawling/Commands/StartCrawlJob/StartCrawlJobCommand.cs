using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace CrawlScope.Application.Modules.Crawling.Commands.StartCrawlJob
{
    public record StartCrawlJobCommand(
        Guid Id,
        string RequestingUserId,
        bool IncludeAllUsers) : IRequest
    {
    }
}
