using CrawlScope.Application.Modules.Crawling.Commands.CreateCrawlSchedule;
using CrawlScope.Application.Modules.Crawling.Commands.SetCrawlScheduleStatus;
using CrawlScope.Application.Modules.Crawling.DTOs;
using CrawlScope.Application.Modules.Crawling.Queries.GetCrawlSchedules;
using CrawlScope.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CrawlScope.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CrawlScheduleController(IMediator mediator) : ControllerBase
    {
        [HttpPost]
        [Authorize(Policy = Permissions.Schedules.Create)]
        public async Task<IActionResult> Create(
            [FromBody] CreateCrawlScheduleRequestDto request,
            CancellationToken cancellationToken)
        {
            var command = new CreateCrawlScheduleCommand(request, "System");
            var id = await mediator.Send(command, cancellationToken);
            return Ok(id);
        }

        [HttpGet]
        [Authorize(Policy = Permissions.Schedules.View)]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var query = new GetCrawlSchedulesQuery();
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
    }
}
