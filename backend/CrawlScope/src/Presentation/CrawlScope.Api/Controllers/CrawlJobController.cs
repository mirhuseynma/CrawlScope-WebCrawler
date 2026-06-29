using CrawlScope.Application.Modules.Crawling.Commands.CreateCrawlJob;
using CrawlScope.Application.Modules.Crawling.Commands.StartCrawlJob;
using CrawlScope.Application.Modules.Crawling.DTOs;
using CrawlScope.Application.Modules.Crawling.Queries.GetCrawledPages;
using CrawlScope.Application.Modules.Crawling.Queries.GetCrawlJobById;
using CrawlScope.Application.Modules.Crawling.Queries.GetCrawlJobs;
using CrawlScope.Application.Modules.Crawling.Queries.GetCrawlLogs;
using CrawlScope.Application.Modules.Export.Commands.ExportCrawledData;
using CrawlScope.Domain.Modules.Crawling.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CrawlScope.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CrawlJobController(IMediator mediator) : ControllerBase
    {

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCrawlJobRequestDto request, CancellationToken cancellationToken)
        {
            var command = new CreateCrawlJobCommand(request, "System");
            var id = await mediator.Send(command, cancellationToken);
            return Ok(id);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var query = new GetCrawlJobsQuery();
            var crawlJobs = await mediator.Send(query, cancellationToken);
            return Ok(crawlJobs);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
        {
            var query = new GetCrawlJobByIdQuery(id);
            var crawlJob = await mediator.Send(query, cancellationToken);
            if (crawlJob == null)
            {
                return NotFound();
            }
            return Ok(crawlJob);
        }

        [HttpPost("{id:guid}/start")]
        public async Task<IActionResult> Start(Guid id, CancellationToken cancellationToken)
        {
            var command = new StartCrawlJobCommand(id);
            await mediator.Send(command, cancellationToken);
            return NoContent();
        }

        [HttpGet("{id:guid}/pages")]
        public async Task<IActionResult> GetPages(
            Guid id,
            [FromQuery] string? search,
            [FromQuery] int? statusCode,
            [FromQuery] int? depthLevel,
            CancellationToken cancellationToken)
        {
            var query = new GetCrawledPagesQuery(id, search, statusCode, depthLevel);
            var pages = await mediator.Send(query, cancellationToken);
            return Ok(pages);
        }

        [HttpGet("{id:guid}/logs")]
        public async Task<IActionResult> GetLogs(
            Guid id,
            [FromQuery] string? level,
            CancellationToken cancellationToken)
        {
            var query = new GetCrawlLogsQuery(id, level);
            var logs = await mediator.Send(query, cancellationToken);
            return Ok(logs);
        }

        [HttpPost("{id:guid}/export")]
        public async Task<IActionResult> Export(
            Guid id,
            [FromQuery] ExportFormat format,
            CancellationToken cancellationToken)
        {
            var command = new ExportCrawledDataCommand(id, format, "System");
            var export = await mediator.Send(command, cancellationToken);

            return File(export.Content, export.ContentType, export.FileName);
        }
    }
}
