using System.Security.Claims;
using CrawlScope.Application.Modules.Crawling.Commands.CreateCrawlJob;
using CrawlScope.Application.Modules.Crawling.Commands.StartCrawlJob;
using CrawlScope.Application.Modules.Crawling.DTOs;
using CrawlScope.Application.Modules.Crawling.Queries.GetCrawledPages;
using CrawlScope.Application.Modules.Crawling.Queries.GetCrawlJobById;
using CrawlScope.Application.Modules.Crawling.Queries.GetCrawlJobs;
using CrawlScope.Application.Modules.Crawling.Queries.GetCrawlLogs;
using CrawlScope.Application.Modules.Export.Commands.ExportCrawledData;
using CrawlScope.Domain.Constants;
using CrawlScope.Domain.Modules.Crawling.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CrawlScope.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CrawlJobController(IMediator mediator) : ControllerBase
    {
        private string CurrentUserId =>
            User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("Authenticated user id was not found.");

        private bool CanAccessAllUsers =>
            User.Claims.Any(claim => claim.Type == "Permission" && claim.Value == Permissions.Admin.Access);

        [HttpPost]
        [Authorize(Policy = Permissions.CrawlJobs.Create)]
        public async Task<IActionResult> Create([FromBody] CreateCrawlJobRequestDto request, CancellationToken cancellationToken)
        {
            var command = new CreateCrawlJobCommand(request, CurrentUserId);
            var id = await mediator.Send(command, cancellationToken);
            return Ok(id);
        }

        [HttpGet]
        [Authorize(Policy = Permissions.CrawlJobs.View)]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search,
            [FromQuery] string? status,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 5,
            CancellationToken cancellationToken = default)
        {
            var query = new GetCrawlJobsQuery(search, status, pageNumber, pageSize, CurrentUserId, CanAccessAllUsers);
            var crawlJobs = await mediator.Send(query, cancellationToken);
            return Ok(crawlJobs);
        }

        [HttpGet("{id:guid}")]
        [Authorize(Policy = Permissions.CrawlJobs.View)]
        public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
        {
            var query = new GetCrawlJobByIdQuery(id, CurrentUserId, CanAccessAllUsers);
            var crawlJob = await mediator.Send(query, cancellationToken);

            if (crawlJob == null)
            {
                return NotFound();
            }

            return Ok(crawlJob);
        }

        [HttpPost("{id:guid}/start")]
        [Authorize(Policy = Permissions.CrawlJobs.Start)]
        public async Task<IActionResult> Start(Guid id, CancellationToken cancellationToken)
        {
            var command = new StartCrawlJobCommand(id, CurrentUserId, CanAccessAllUsers);
            await mediator.Send(command, cancellationToken);
            return NoContent();
        }

        [HttpGet("{id:guid}/pages")]
        [Authorize(Policy = Permissions.CrawledPages.View)]
        public async Task<IActionResult> GetPages(
            Guid id,
            [FromQuery] string? search,
            [FromQuery] int? statusCode,
            [FromQuery] int? depthLevel,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 5,
            CancellationToken cancellationToken = default)
        {
            var query = new GetCrawledPagesQuery(
                id,
                search,
                statusCode,
                depthLevel,
                pageNumber,
                pageSize,
                CurrentUserId,
                CanAccessAllUsers);
            var pages = await mediator.Send(query, cancellationToken);
            return Ok(pages);
        }

        [HttpGet("pages")]
        [Authorize(Policy = Permissions.CrawledPages.View)]
        public async Task<IActionResult> GetAllPages(
            [FromQuery] string? search,
            [FromQuery] int? statusCode,
            [FromQuery] int? depthLevel,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 5,
            CancellationToken cancellationToken = default)
        {
            var query = new GetCrawledPagesQuery(
                null,
                search,
                statusCode,
                depthLevel,
                pageNumber,
                pageSize,
                CurrentUserId,
                CanAccessAllUsers);
            var pages = await mediator.Send(query, cancellationToken);
            return Ok(pages);
        }

        [HttpGet("{id:guid}/logs")]
        [Authorize(Policy = Permissions.CrawlJobs.View)]
        public async Task<IActionResult> GetLogs(
            Guid id,
            [FromQuery] string? level,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            var query = new GetCrawlLogsQuery(id, level, pageNumber, pageSize, CurrentUserId, CanAccessAllUsers);
            var logs = await mediator.Send(query, cancellationToken);
            return Ok(logs);
        }

        [HttpPost("{id:guid}/export")]
        [Authorize(Policy = Permissions.CrawlJobs.Export)]
        public async Task<IActionResult> Export(
            Guid id,
            [FromQuery] ExportFormat format,
            CancellationToken cancellationToken)
        {
            var command = new ExportCrawledDataCommand(id, format, CurrentUserId, CanAccessAllUsers);
            var export = await mediator.Send(command, cancellationToken);

            return File(export.Content, export.ContentType, export.FileName);
        }
    }
}
