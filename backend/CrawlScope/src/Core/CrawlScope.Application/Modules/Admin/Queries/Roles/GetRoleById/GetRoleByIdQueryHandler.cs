namespace CrawlScope.Application.Modules.Admin.Queries.Roles.GetRoleById
{
    public class GetRoleByIdQueryHandler(
        RoleManager<IdentityRole> roleManager,
        UserManager<AppUser> userManager) : IRequestHandler<GetRoleByIdQuery, RoleDetailsDto>
    {
        private const string PermissionClaimType = "Permission";
        private static readonly HashSet<string> ProtectedRoles = new(StringComparer.OrdinalIgnoreCase) { "Admin", "User" };

        public async Task<RoleDetailsDto> Handle(GetRoleByIdQuery request, CancellationToken cancellationToken)
        {
            var role = await FindRoleByIdAsync(request.RoleId);
            return await ToDetailsDtoAsync(role);
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
    }
}
