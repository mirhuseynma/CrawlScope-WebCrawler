namespace CrawlScope.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = Permissions.Admin.Access)]
    public class AdminUsersController(IMediator mediator) : ControllerBase
    {
        [HttpGet]
        [Authorize(Policy = Permissions.Users.View)]
        public async Task<IActionResult> GetUsers(
            [FromQuery] string? search,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = new GetUsersQuery { Search = search, PageNumber = pageNumber, PageSize = pageSize };
            var users = await mediator.Send(query);
            return Ok(users);
        }

        [HttpGet("{id}")]
        [Authorize(Policy = Permissions.Users.View)]
        public async Task<IActionResult> GetById(string id)
        {
            var query = new GetUserByIdQuery { UserId = id };
            var user = await mediator.Send(query);
            return Ok(user);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = Permissions.Users.Manage)]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateUserRequestDto request)
        {
            var command = new UpdateUserCommand { UserId = id, Dto = request };
            var user = await mediator.Send(command);
            return Ok(user);
        }

        [HttpPut("{id}/roles")]
        [Authorize(Policy = Permissions.Users.Manage)]
        public async Task<IActionResult> UpdateRoles(string id, [FromBody] UpdateUserRolesRequestDto request)
        {
            var command = new UpdateUserRolesCommand { UserId = id, Dto = request };
            var user = await mediator.Send(command);
            return Ok(user);
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = Permissions.Users.Manage)]
        public async Task<IActionResult> Delete(string id)
        {
            var command = new DeleteUserCommand { UserId = id };
            await mediator.Send(command);
            return NoContent();
        }
    }
}
