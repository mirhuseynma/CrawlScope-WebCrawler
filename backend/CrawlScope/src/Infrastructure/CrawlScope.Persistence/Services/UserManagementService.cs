
namespace CrawlScope.Persistence.Services
{
    public class UserManagementService(
        UserManager<AppUser> userManager,
        RoleManager<IdentityRole> roleManager) : IUserManagementService
    {
        private const string AdminRoleName = "Admin";
        private const string PermissionClaimType = "Permission";

        public async Task<PagedResult<UserListItemDto>> GetUsersAsync(string? search, int pageNumber, int pageSize)
        {
            var query = userManager.Users.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var trimmedSearch = search.Trim();
                query = query.Where(user =>
                    (user.UserName != null && user.UserName.Contains(trimmedSearch))
                    || (user.Email != null && user.Email.Contains(trimmedSearch))
                    || (user.FullName != null && user.FullName.Contains(trimmedSearch)));
            }

            var pagedUsers = await PagedResult<AppUser>.CreateAsync(
                query.OrderBy(user => user.UserName),
                pageNumber,
                pageSize,
                CancellationToken.None);

            var items = new List<UserListItemDto>();

            foreach (var user in pagedUsers.Items)
            {
                var roles = await userManager.GetRolesAsync(user);
                items.Add(new UserListItemDto
                {
                    Id = user.Id,
                    UserName = user.UserName ?? string.Empty,
                    Email = user.Email ?? string.Empty,
                    FullName = user.FullName,
                    Roles = roles.ToArray(),
                    IsSystemManaged = await IsSystemManagedUserAsync(user)
                });
            }

            return new PagedResult<UserListItemDto>
            {
                Items = items,
                PageNumber = pagedUsers.PageNumber,
                PageSize = pagedUsers.PageSize,
                TotalCount = pagedUsers.TotalCount,
                TotalPages = pagedUsers.TotalPages
            };
        }

        public async Task<UserDetailsDto> GetUserByIdAsync(string userId)
        {
            var user = await FindUserByIdAsync(userId);
            return await ToDetailsDtoAsync(user);
        }

        public async Task<UserDetailsDto> UpdateUserAsync(string userId, UpdateUserRequestDto request)
        {
            var user = await FindUserByIdAsync(userId);

            if (await IsSystemManagedUserAsync(user))
            {
                throw new BadRequestException("System admin user can only be managed from seed configuration.");
            }

            var userName = NormalizeRequired(request.UserName, "User name");
            var email = NormalizeRequired(request.Email, "Email");

            if (!string.Equals(user.UserName, userName, StringComparison.OrdinalIgnoreCase))
            {
                var userNameResult = await userManager.SetUserNameAsync(user, userName);

                if (!userNameResult.Succeeded)
                {
                    ThrowIdentityErrors("User name update failed", userNameResult);
                }
            }

            if (!string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase))
            {
                var emailResult = await userManager.SetEmailAsync(user, email);

                if (!emailResult.Succeeded)
                {
                    ThrowIdentityErrors("Email update failed", emailResult);
                }
            }

            user.FullName = string.IsNullOrWhiteSpace(request.FullName)
                ? null
                : request.FullName.Trim();

            var updateResult = await userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
            {
                ThrowIdentityErrors("User update failed", updateResult);
            }

            return await ToDetailsDtoAsync(user);
        }

        public async Task<UserDetailsDto> UpdateUserRolesAsync(string userId, UpdateUserRolesRequestDto request)
        {
            var user = await FindUserByIdAsync(userId);

            if (await IsSystemManagedUserAsync(user))
            {
                throw new BadRequestException("System admin roles can only be managed from seed configuration.");
            }

            var requestedRoles = request.Roles
                .Where(role => !string.IsNullOrWhiteSpace(role))
                .Select(role => role.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            foreach (var roleName in requestedRoles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    throw new BadRequestException($"Role '{roleName}' does not exist.");
                }
            }

            var currentRoles = await userManager.GetRolesAsync(user);
            var rolesToRemove = currentRoles
                .Where(role => !requestedRoles.Contains(role, StringComparer.OrdinalIgnoreCase))
                .ToArray();
            var rolesToAdd = requestedRoles
                .Where(role => !currentRoles.Contains(role, StringComparer.OrdinalIgnoreCase))
                .ToArray();

            if (rolesToRemove.Contains(AdminRoleName, StringComparer.OrdinalIgnoreCase))
            {
                await EnsureAnotherAdminExistsAsync(user.Id);
            }

            if (rolesToRemove.Length > 0)
            {
                var removeResult = await userManager.RemoveFromRolesAsync(user, rolesToRemove);

                if (!removeResult.Succeeded)
                {
                    ThrowIdentityErrors("Role removal failed", removeResult);
                }
            }

            if (rolesToAdd.Length > 0)
            {
                var addResult = await userManager.AddToRolesAsync(user, rolesToAdd);

                if (!addResult.Succeeded)
                {
                    ThrowIdentityErrors("Role assignment failed", addResult);
                }
            }

            return await ToDetailsDtoAsync(user);
        }

        public async Task DeleteUserAsync(string userId)
        {
            var user = await FindUserByIdAsync(userId);

            if (await IsSystemManagedUserAsync(user))
            {
                throw new BadRequestException("System admin user cannot be deleted.");
            }

            if (await userManager.IsInRoleAsync(user, AdminRoleName))
            {
                await EnsureAnotherAdminExistsAsync(user.Id);
            }

            var result = await userManager.DeleteAsync(user);

            if (!result.Succeeded)
            {
                ThrowIdentityErrors("User deletion failed", result);
            }
        }

        private async Task<AppUser> FindUserByIdAsync(string userId)
        {
            return await userManager.FindByIdAsync(userId)
                ?? throw new NotFoundException("User not found.");
        }

        private async Task<UserDetailsDto> ToDetailsDtoAsync(AppUser user)
        {
            var roles = await userManager.GetRolesAsync(user);
            var permissions = await GetPermissionsAsync(roles);

            return new UserDetailsDto
            {
                Id = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                FullName = user.FullName,
                Roles = roles.ToArray(),
                Permissions = permissions,
                IsSystemManaged = await IsSystemManagedUserAsync(user)
            };
        }

        private async Task<IReadOnlyCollection<string>> GetPermissionsAsync(IEnumerable<string> roles)
        {
            var permissions = new HashSet<string>();

            foreach (var roleName in roles)
            {
                var role = await roleManager.FindByNameAsync(roleName);

                if (role is null)
                {
                    continue;
                }

                var claims = await roleManager.GetClaimsAsync(role);

                foreach (var claim in claims.Where(claim => claim.Type == PermissionClaimType))
                {
                    permissions.Add(claim.Value);
                }
            }

            return permissions.OrderBy(permission => permission).ToArray();
        }

        private async Task<bool> IsSystemManagedUserAsync(AppUser user)
        {
            var claims = await userManager.GetClaimsAsync(user);
            return claims.Any(claim =>
                claim.Type == SystemClaims.SystemUser && claim.Value == SystemClaims.SeedAdmin);
        }

        private async Task EnsureAnotherAdminExistsAsync(string currentUserId)
        {
            var adminUsers = await userManager.GetUsersInRoleAsync(AdminRoleName);
            var hasAnotherAdmin = adminUsers.Any(user => user.Id != currentUserId);

            if (!hasAnotherAdmin)
            {
                throw new BadRequestException("At least one admin user must remain.");
            }
        }

        private static string NormalizeRequired(string value, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new BadRequestException($"{fieldName} is required.");
            }

            return value.Trim();
        }

        private static void ThrowIdentityErrors(string message, IdentityResult result)
        {
            var errors = string.Join(", ", result.Errors.Select(error => error.Description));
            throw new BadRequestException($"{message}: {errors}");
        }
    }
}
