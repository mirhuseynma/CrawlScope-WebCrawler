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
    public class AdminRolesController(IRoleManagementService roleManagementService) : ControllerBase
    {
        [HttpGet("permissions")]
        [Authorize(Policy = Permissions.Roles.View)]
        public async Task<IActionResult> GetPermissions()
        {
            var permissions = await roleManagementService.GetPermissionsAsync();
            return Ok(permissions);
        }

        [HttpGet]
        [Authorize(Policy = Permissions.Roles.View)]
        public async Task<IActionResult> GetRoles()
        {
            var roles = await roleManagementService.GetRolesAsync();
            return Ok(roles);
        }

        [HttpGet("{id}")]
        [Authorize(Policy = Permissions.Roles.View)]
        public async Task<IActionResult> GetById(string id)
        {
            var role = await roleManagementService.GetRoleByIdAsync(id);
            return Ok(role);
        }

        [HttpPost]
        [Authorize(Policy = Permissions.Roles.Manage)]
        public async Task<IActionResult> Create([FromBody] CreateRoleRequestDto request)
        {
            var role = await roleManagementService.CreateRoleAsync(request);
            return Ok(role);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = Permissions.Roles.Manage)]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateRoleRequestDto request)
        {
            var role = await roleManagementService.UpdateRoleAsync(id, request);
            return Ok(role);
        }

        [HttpPut("{id}/permissions")]
        [Authorize(Policy = Permissions.Roles.Manage)]
        public async Task<IActionResult> UpdatePermissions(string id, [FromBody] UpdateRolePermissionsRequestDto request)
        {
            var role = await roleManagementService.UpdateRolePermissionsAsync(id, request);
            return Ok(role);
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = Permissions.Roles.Manage)]
        public async Task<IActionResult> Delete(string id)
        {
            await roleManagementService.DeleteRoleAsync(id);
            return NoContent();
        }
    }
}
