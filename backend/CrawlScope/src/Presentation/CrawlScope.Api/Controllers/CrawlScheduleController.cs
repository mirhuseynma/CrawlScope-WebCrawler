
namespace CrawlScope.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CrawlScheduleController(IMediator mediator) : ApiControllerBase
    {
        [HttpPost]
        [Authorize(Policy = Permissions.Schedules.Create)]
        public async Task<IActionResult> Create(
            [FromBody] CreateCrawlScheduleRequestDto request,
            CancellationToken cancellationToken)
        {
            var command = new CreateCrawlScheduleCommand(request, CurrentUserId);
            var id = await mediator.Send(command, cancellationToken);
            return Ok(id);
        }

        [HttpGet]
        [Authorize(Policy = Permissions.Schedules.View)]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search,
            [FromQuery] bool? isEnabled,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 5,
            CancellationToken cancellationToken = default)
        {
            var query = new GetCrawlSchedulesQuery(search, isEnabled, pageNumber, pageSize);
            var schedules = await mediator.Send(query, cancellationToken);
            return Ok(schedules);
        }

        [HttpPatch("{id:guid}/enable")]
        [Authorize(Policy = Permissions.Schedules.Manage)]
        public async Task<IActionResult> Enable(Guid id, CancellationToken cancellationToken)
        {
            var command = new SetCrawlScheduleStatusCommand(id, true);
            await mediator.Send(command, cancellationToken);
            return NoContent();
        }

        [HttpPatch("{id:guid}/disable")]
        [Authorize(Policy = Permissions.Schedules.Manage)]
        public async Task<IActionResult> Disable(Guid id, CancellationToken cancellationToken)
        {
            var command = new SetCrawlScheduleStatusCommand(id, false);
            await mediator.Send(command, cancellationToken);
            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Policy = Permissions.Schedules.Manage)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            var command = new DeleteCrawlScheduleCommand(id);
            await mediator.Send(command, cancellationToken);
            return NoContent();
        }
    }
}
