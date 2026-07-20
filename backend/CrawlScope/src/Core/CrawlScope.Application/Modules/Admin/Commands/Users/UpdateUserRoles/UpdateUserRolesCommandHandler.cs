namespace CrawlScope.Application.Modules.Admin.Commands.Users.UpdateUserRoles
{
    public class UpdateUserRolesCommandHandler(
        UserManager<AppUser> userManager,
        RoleManager<IdentityRole> roleManager) : IRequestHandler<UpdateUserRolesCommand, UserDetailsDto>
    {
        private const string AdminRoleName = "Admin";
        private const string PermissionClaimType = "Permission";

        public async Task<UserDetailsDto> Handle(UpdateUserRolesCommand request, CancellationToken cancellationToken)
        {
            var user = await FindUserByIdAsync(request.UserId);

            if (await IsSystemManagedUserAsync(user))
            {
                throw new BadRequestException("System admin roles can only be managed from seed configuration.");
            }

            var requestedRoles = request.Dto.Roles
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

        private static void ThrowIdentityErrors(string message, IdentityResult result)
        {
            var errors = string.Join(", ", result.Errors.Select(error => error.Description));
            throw new BadRequestException($"{message}: {errors}");
        }
    }
}
