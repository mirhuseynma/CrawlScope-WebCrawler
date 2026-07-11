
namespace CrawlScope.Application.Abstractions.Auth
{
    public interface IRoleManagementService
    {
        Task<IReadOnlyCollection<PermissionDto>> GetPermissionsAsync();
        Task<IReadOnlyCollection<RoleListItemDto>> GetRolesAsync();
        Task<RoleDetailsDto> GetRoleByIdAsync(string roleId);
        Task<RoleDetailsDto> CreateRoleAsync(CreateRoleRequestDto request);
        Task<RoleDetailsDto> UpdateRoleAsync(string roleId, UpdateRoleRequestDto request);
        Task<RoleDetailsDto> UpdateRolePermissionsAsync(string roleId, UpdateRolePermissionsRequestDto request);
        Task DeleteRoleAsync(string roleId);
    }
}
