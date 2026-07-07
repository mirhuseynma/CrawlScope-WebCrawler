using CrawlScope.Application.Modules.Admin.Queries.GetAdminOverview;
using CrawlScope.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CrawlScope.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = Permissions.Admin.Access)]
    public class AdminOverviewController(IMediator mediator) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Get(CancellationToken cancellationToken)
        {
            var overview = await mediator.Send(new GetAdminOverviewQuery(), cancellationToken);
            return Ok(overview);
        }
    }
}
