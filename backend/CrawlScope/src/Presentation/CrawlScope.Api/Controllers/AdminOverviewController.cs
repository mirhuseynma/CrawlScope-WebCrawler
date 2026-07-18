
namespace CrawlScope.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = Permissions.Admin.Access)]
    public class AdminOverviewController(IMediator mediator) : ApiControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Get(CancellationToken cancellationToken)
        {
            var overview = await mediator.Send(new GetAdminOverviewQuery(), cancellationToken);
            return Ok(overview);
        }
    }
}
