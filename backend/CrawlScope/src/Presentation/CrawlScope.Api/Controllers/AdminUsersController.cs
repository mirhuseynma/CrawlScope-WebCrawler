using CrawlScope.Application.Abstractions.Auth;
using CrawlScope.Application.Modules.Auth.DTOs;
using CrawlScope.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CrawlScope.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = Permissions.Admin.Access)]
    public class AdminUsersController(IUserManagementService userManagementService) : ControllerBase
    {
        [HttpGet]
        [Authorize(Policy = Permissions.Users.View)]
        public async Task<IActionResult> GetUsers(
            [FromQuery] string? search,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var users = await userManagementService.GetUsersAsync(search, pageNumber, pageSize);
            return Ok(users);
        }

        [HttpGet("{id}")]
        [Authorize(Policy = Permissions.Users.View)]
        public async Task<IActionResult> GetById(string id)
        {
            var user = await userManagementService.GetUserByIdAsync(id);
            return Ok(user);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = Permissions.Users.Manage)]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateUserRequestDto request)
        {
            var user = await userManagementService.UpdateUserAsync(id, request);
            return Ok(user);
        }

        [HttpPut("{id}/roles")]
        [Authorize(Policy = Permissions.Users.Manage)]
        public async Task<IActionResult> UpdateRoles(string id, [FromBody] UpdateUserRolesRequestDto request)
        {
            var user = await userManagementService.UpdateUserRolesAsync(id, request);
            return Ok(user);
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = Permissions.Users.Manage)]
        public async Task<IActionResult> Delete(string id)
        {
            await userManagementService.DeleteUserAsync(id);
            return NoContent();
        }
    }
}
