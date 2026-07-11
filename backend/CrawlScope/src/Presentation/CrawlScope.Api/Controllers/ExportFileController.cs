using CrawlScope.Application.Modules.Export.Commands.DeleteExportFile;
using CrawlScope.Application.Modules.Export.Queries.DownloadExportFile;
using CrawlScope.Application.Modules.Export.Queries.GetExportFiles;
using CrawlScope.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CrawlScope.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExportFileController(IMediator mediator) : ApiControllerBase
    {
        [HttpGet]
        [Authorize(Policy = Permissions.CrawlJobs.Export)]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search,
            [FromQuery] string? format,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 5,
            CancellationToken cancellationToken = default)
        {
            var query = new GetExportFilesQuery(search, format, pageNumber, pageSize, CurrentUserId, CanAccessAllUsers);
            var exports = await mediator.Send(query, cancellationToken);
            return Ok(exports);
        }

        [HttpGet("{id:guid}/download")]
        [Authorize(Policy = Permissions.CrawlJobs.Export)]
        public async Task<IActionResult> Download(Guid id, CancellationToken cancellationToken)
        {
            var query = new DownloadExportFileQuery(id, CurrentUserId, CanAccessAllUsers);
            var export = await mediator.Send(query, cancellationToken);

            return File(export.Content, export.ContentType, export.FileName);
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Policy = Permissions.CrawlJobs.Export)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            var command = new DeleteExportFileCommand(id, CurrentUserId, CanAccessAllUsers);
            await mediator.Send(command, cancellationToken);
            return NoContent();
        }
    }
}
