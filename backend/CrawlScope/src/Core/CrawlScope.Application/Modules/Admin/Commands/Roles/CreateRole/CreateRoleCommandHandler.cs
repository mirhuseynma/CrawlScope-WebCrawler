namespace CrawlScope.Application.Modules.Admin.Commands.Roles.CreateRole
{
    public class CreateRoleCommandHandler(
        RoleManager<IdentityRole> roleManager,
        UserManager<AppUser> userManager) : IRequestHandler<CreateRoleCommand, RoleDetailsDto>
    {
        private const string PermissionClaimType = "Permission";
        private static readonly HashSet<string> ProtectedRoles = new(StringComparer.OrdinalIgnoreCase) { "Admin", "User" };

        public async Task<RoleDetailsDto> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
        {
            var roleName = NormalizeRoleName(request.Dto.Name);

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

            await ReplacePermissionsAsync(role, request.Dto.Permissions);

            return await ToDetailsDtoAsync(role);
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

        private static void ThrowIdentityErrors(string message, IdentityResult result)
        {
            var errors = string.Join(", ", result.Errors.Select(error => error.Description));
            throw new BadRequestException($"{message}: {errors}");
        }
    }
}
