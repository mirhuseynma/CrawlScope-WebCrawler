namespace CrawlScope.Persistence.Services
{
    public class RoleManagementService(
        RoleManager<IdentityRole> roleManager,
        UserManager<AppUser> userManager) : IRoleManagementService
    {
        private const string PermissionClaimType = "Permission";
        private static readonly HashSet<string> ProtectedRoles = new(StringComparer.OrdinalIgnoreCase) { "Admin", "User" };

        public Task<IReadOnlyCollection<PermissionDto>> GetPermissionsAsync()
        {
            IReadOnlyCollection<PermissionDto> permissions = [.. Permissions.All()
                .OrderBy(GetPermissionGroup)
                .ThenBy(GetPermissionName)
                .Select(permission => new PermissionDto
                {
                    Value = permission,
                    Group = GetPermissionGroup(permission),
                    Name = GetPermissionName(permission)
                })];

            return Task.FromResult(permissions);
        }

        public async Task<IReadOnlyCollection<RoleListItemDto>> GetRolesAsync()
        {
            var roles = await roleManager.Roles
                .OrderBy(role => role.Name)
                .ToListAsync();

            var result = new List<RoleListItemDto>();

            foreach (var role in roles)
            {
                var permissions = await GetRolePermissionsAsync(role);
                result.Add(new RoleListItemDto
                {
                    Id = role.Id,
                    Name = role.Name ?? string.Empty,
                    Permissions = permissions,
                    UserCount = await CountUsersInRoleAsync(role.Name),
                    IsSystemManaged = IsProtectedRole(role.Name)
                });
            }

            return result;
        }

        public async Task<RoleDetailsDto> GetRoleByIdAsync(string roleId)
        {
            var role = await FindRoleByIdAsync(roleId);
            return await ToDetailsDtoAsync(role);
        }

        public async Task<RoleDetailsDto> CreateRoleAsync(CreateRoleRequestDto request)
        {
            var roleName = NormalizeRoleName(request.Name);

            if (await roleManager.RoleExistsAsync(roleName))
            {
                throw new BadRequestException($"Role '{roleName}' already exists.");
            }

            var role = new IdentityRole(roleName);
            var createResult = await roleManager.CreateAsync(role);

            if (!createResult.Succeeded)
            {
                ThrowIdentityErrors("Role creation failed", createResult);
            }

            await ReplacePermissionsAsync(role, request.Permissions);

            return await ToDetailsDtoAsync(role);
        }

        public async Task<RoleDetailsDto> UpdateRoleAsync(string roleId, UpdateRoleRequestDto request)
        {
            var role = await FindRoleByIdAsync(roleId);
            var currentRoleName = role.Name ?? string.Empty;
            var nextRoleName = NormalizeRoleName(request.Name);

            if (!string.Equals(currentRoleName, nextRoleName, StringComparison.OrdinalIgnoreCase))
            {
                if (IsProtectedRole(currentRoleName))
                {
                    throw new BadRequestException($"System role '{currentRoleName}' cannot be renamed.");
                }

                var existingRole = await roleManager.FindByNameAsync(nextRoleName);

                if (existingRole is not null && existingRole.Id != role.Id)
                {
                    throw new BadRequestException($"Role '{nextRoleName}' already exists.");
                }

                role.Name = nextRoleName;
                role.NormalizedName = roleManager.NormalizeKey(nextRoleName);

                var updateResult = await roleManager.UpdateAsync(role);

                if (!updateResult.Succeeded)
                {
                    ThrowIdentityErrors("Role update failed", updateResult);
                }
            }

            await ReplacePermissionsAsync(role, request.Permissions);

            return await ToDetailsDtoAsync(role);
        }

        public async Task<RoleDetailsDto> UpdateRolePermissionsAsync(string roleId, UpdateRolePermissionsRequestDto request)
        {
            var role = await FindRoleByIdAsync(roleId);
            await ReplacePermissionsAsync(role, request.Permissions);

            return await ToDetailsDtoAsync(role);
        }

        public async Task DeleteRoleAsync(string roleId)
        {
            var role = await FindRoleByIdAsync(roleId);
            var roleName = role.Name ?? string.Empty;

            if (IsProtectedRole(roleName))
            {
                throw new BadRequestException($"System role '{roleName}' cannot be deleted.");
            }

            if (await CountUsersInRoleAsync(roleName) > 0)
            {
                throw new BadRequestException($"Role '{roleName}' is assigned to users and cannot be deleted.");
            }

            var result = await roleManager.DeleteAsync(role);

            if (!result.Succeeded)
            {
                ThrowIdentityErrors("Role deletion failed", result);
            }
        }

        private async Task<IdentityRole> FindRoleByIdAsync(string roleId)
        {
            return await roleManager.FindByIdAsync(roleId)
                ?? throw new NotFoundException("Role not found.");
        }

        private async Task<RoleDetailsDto> ToDetailsDtoAsync(IdentityRole role)
        {
            return new RoleDetailsDto
            {
                Id = role.Id,
                Name = role.Name ?? string.Empty,
                Permissions = await GetRolePermissionsAsync(role),
                UserCount = await CountUsersInRoleAsync(role.Name),
                IsSystemManaged = IsProtectedRole(role.Name)
            };
        }

        private async Task<IReadOnlyCollection<string>> GetRolePermissionsAsync(IdentityRole role)
        {
            var claims = await roleManager.GetClaimsAsync(role);
            return [.. claims
                .Where(claim => claim.Type == PermissionClaimType)
                .Select(claim => claim.Value)
                .OrderBy(permission => permission)];
        }

        private async Task ReplacePermissionsAsync(IdentityRole role, IEnumerable<string> requestedPermissions)
        {
            var validPermissions = Permissions.All().ToHashSet();
            var permissionsSource = string.Equals(role.Name, "Admin", StringComparison.OrdinalIgnoreCase)
                ? Permissions.All()
                : requestedPermissions;
            var permissions = permissionsSource
                .Where(permission => !string.IsNullOrWhiteSpace(permission))
                .Select(permission => permission.Trim())
                .Distinct()
                .ToArray();

            var invalidPermissions = permissions
                .Where(permission => !validPermissions.Contains(permission))
                .ToArray();

            if (invalidPermissions.Length > 0)
            {
                throw new BadRequestException($"Invalid permissions: {string.Join(", ", invalidPermissions)}.");
            }

            var existingClaims = await roleManager.GetClaimsAsync(role);
            var permissionClaims = existingClaims
                .Where(claim => claim.Type == PermissionClaimType)
                .ToArray();

            foreach (var claim in permissionClaims.Where(claim => !permissions.Contains(claim.Value)))
            {
                var removeResult = await roleManager.RemoveClaimAsync(role, claim);

                if (!removeResult.Succeeded)
                {
                    ThrowIdentityErrors("Permission removal failed", removeResult);
                }
            }

            var existingPermissions = permissionClaims.Select(claim => claim.Value).ToHashSet();

            foreach (var permission in permissions.Where(permission => !existingPermissions.Contains(permission)))
            {
                var addResult = await roleManager.AddClaimAsync(role, new Claim(PermissionClaimType, permission));

                if (!addResult.Succeeded)
                {
                    ThrowIdentityErrors("Permission assignment failed", addResult);
                }
            }
        }

        private async Task<int> CountUsersInRoleAsync(string? roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
            {
                return 0;
            }

            var users = await userManager.GetUsersInRoleAsync(roleName);
            return users.Count;
        }

        private static bool IsProtectedRole(string? roleName)
        {
            return !string.IsNullOrWhiteSpace(roleName) && ProtectedRoles.Contains(roleName);
        }

        private static string NormalizeRoleName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new BadRequestException("Role name is required.");
            }

            return name.Trim();
        }

        private static string GetPermissionGroup(string permission)
        {
            var parts = permission.Split('.');
            return parts.Length >= 2 ? parts[1] : "General";
        }

        private static string GetPermissionName(string permission)
        {
            var parts = permission.Split('.');
            return parts.Length >= 3 ? parts[2] : permission;
        }

        private static void ThrowIdentityErrors(string message, IdentityResult result)
        {
            var errors = string.Join(", ", result.Errors.Select(error => error.Description));
            throw new BadRequestException($"{message}: {errors}");
        }
    }
}
