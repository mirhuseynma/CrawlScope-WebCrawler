
namespace CrawlScope.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CrawlJobController(IMediator mediator) : ApiControllerBase
    {
        [HttpPost]
        [Authorize(Policy = Permissions.CrawlJobs.Create)]
        public async Task<IActionResult> Create([FromBody] CreateCrawlJobRequestDto request, CancellationToken cancellationToken)
        {
            var command = new CreateCrawlJobCommand(request, CurrentUserId);
            var id = await mediator.Send(command, cancellationToken);
            return Ok(id);
        }

        [HttpGet("analyze")]
        [Authorize(Policy = Permissions.CrawlJobs.Create)]
        public async Task<IActionResult> AnalyzeUrl([FromQuery] string url, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return BadRequest("URL is required.");
            }

            var query = new CrawlScope.Application.Modules.Crawling.Queries.AnalyzeUrl.AnalyzeUrlQuery(url);
            var result = await mediator.Send(query, cancellationToken);
            return Ok(result);
        }

        [HttpGet]
        [Authorize(Policy = Permissions.CrawlJobs.View)]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search,
            [FromQuery] string? status,
            [FromQuery] bool? importantOnly,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 5,
            CancellationToken cancellationToken = default)
        {
            var query = new GetCrawlJobsQuery(search, status, importantOnly, pageNumber, pageSize, CurrentUserId, CanAccessAllUsers);
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

        [HttpPost("{id:guid}/cancel")]
        [Authorize(Policy = Permissions.CrawlJobs.Start)]
        public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
        {
            var command = new CancelCrawlJobCommand(id, CurrentUserId, CanAccessAllUsers);
            await mediator.Send(command, cancellationToken);
            return NoContent();
        }

        [HttpPatch("{id:guid}/importance")]
        [Authorize(Policy = Permissions.Admin.Access)]
        public async Task<IActionResult> ToggleImportance(Guid id, CancellationToken cancellationToken)
        {
            var isImportant = await mediator.Send(new ToggleCrawlJobImportanceCommand(id), cancellationToken);
            return Ok(new { isImportant });
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Policy = Permissions.CrawlJobs.View)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            var command = new DeleteCrawlJobCommand(id, CurrentUserId, CanAccessAllUsers);
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

        [HttpGet("{id:guid}/broken-links")]
        [Authorize(Policy = Permissions.CrawledPages.View)]
        public async Task<IActionResult> GetBrokenLinks(
            Guid id,
            [FromQuery] string? search,
            [FromQuery] int? statusCode,
            [FromQuery] bool? externalOnly,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 5,
            CancellationToken cancellationToken = default)
        {
            var query = new GetBrokenLinksQuery(
                id,
                search,
                statusCode,
                externalOnly,
                pageNumber,
                pageSize,
                CurrentUserId,
                CanAccessAllUsers);
            var brokenLinks = await mediator.Send(query, cancellationToken);
            return Ok(brokenLinks);
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

            return PhysicalFile(export.FilePath, export.ContentType, export.FileName);
        }
    }
}
