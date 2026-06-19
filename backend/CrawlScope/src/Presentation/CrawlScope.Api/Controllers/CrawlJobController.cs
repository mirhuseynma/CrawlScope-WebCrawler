using CrawlScope.Application.Modules.Crawling.Commands.CreateCrawlJob;
using CrawlScope.Application.Modules.Crawling.DTOs;
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
    }
}
