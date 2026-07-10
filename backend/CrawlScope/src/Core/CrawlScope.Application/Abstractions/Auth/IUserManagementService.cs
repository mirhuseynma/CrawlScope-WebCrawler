using CrawlScope.Application.Common.Pagination;
using CrawlScope.Application.Modules.Auth.DTOs;

namespace CrawlScope.Application.Abstractions.Auth
{
    public interface IUserManagementService
    {
        Task<PagedResult<UserListItemDto>> GetUsersAsync(string? search, int pageNumber, int pageSize);
        Task<UserDetailsDto> GetUserByIdAsync(string userId);
        Task<UserDetailsDto> UpdateUserAsync(string userId, UpdateUserRequestDto request);
        Task<UserDetailsDto> UpdateUserRolesAsync(string userId, UpdateUserRolesRequestDto request);
        Task DeleteUserAsync(string userId);
    }
}
