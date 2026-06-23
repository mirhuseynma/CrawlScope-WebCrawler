using CrawlScope.Application.Modules.Crawling.Commands.CreateCrawlJob;
using CrawlScope.Application.Modules.Crawling.Commands.StartCrawlJob;
using CrawlScope.Application.Modules.Crawling.DTOs;
using CrawlScope.Application.Modules.Crawling.Queries.GetCrawlJobById;
using MediatR;
using Microsoft.AspNetCore.Http;
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
            var command = new CreateCrawlJobCommand(request,"System");
            var id = await mediator.Send(command, cancellationToken);
            return Ok(id);
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
    }
}
