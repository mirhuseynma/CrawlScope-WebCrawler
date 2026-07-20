namespace CrawlScope.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = Permissions.Admin.Access)]
    public class AdminRolesController(IMediator mediator) : ControllerBase
    {
        [HttpGet("permissions")]
        [Authorize(Policy = Permissions.Roles.View)]
        public async Task<IActionResult> GetPermissions()
        {
            var query = new GetPermissionsQuery();
            var permissions = await mediator.Send(query);
            return Ok(permissions);
        }

        [HttpGet]
        [Authorize(Policy = Permissions.Roles.View)]
        public async Task<IActionResult> GetRoles()
        {
            var query = new GetRolesQuery();
            var roles = await mediator.Send(query);
            return Ok(roles);
        }

        [HttpGet("{id}")]
        [Authorize(Policy = Permissions.Roles.View)]
        public async Task<IActionResult> GetById(string id)
        {
            var query = new GetRoleByIdQuery { RoleId = id };
            var role = await mediator.Send(query);
            return Ok(role);
        }

        [HttpPost]
        [Authorize(Policy = Permissions.Roles.Manage)]
        public async Task<IActionResult> Create([FromBody] CreateRoleRequestDto request)
        {
            var command = new CreateRoleCommand { Dto = request };
            var role = await mediator.Send(command);
            return Ok(role);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = Permissions.Roles.Manage)]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateRoleRequestDto request)
        {
            var command = new UpdateRoleCommand { RoleId = id, Dto = request };
            var role = await mediator.Send(command);
            return Ok(role);
        }

        [HttpPut("{id}/permissions")]
        [Authorize(Policy = Permissions.Roles.Manage)]
        public async Task<IActionResult> UpdatePermissions(string id, [FromBody] UpdateRolePermissionsRequestDto request)
        {
            var command = new UpdateRolePermissionsCommand { RoleId = id, Dto = request };
            var role = await mediator.Send(command);
            return Ok(role);
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = Permissions.Roles.Manage)]
        public async Task<IActionResult> Delete(string id)
        {
            var command = new DeleteRoleCommand { RoleId = id };
            await mediator.Send(command);
            return NoContent();
        }
    }
}
