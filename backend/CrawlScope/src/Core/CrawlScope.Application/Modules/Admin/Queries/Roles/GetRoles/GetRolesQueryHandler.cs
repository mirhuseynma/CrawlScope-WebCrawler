namespace CrawlScope.Application.Modules.Admin.Queries.Roles.GetRoles
{
    public class GetRolesQueryHandler(
        RoleManager<IdentityRole> roleManager,
        UserManager<AppUser> userManager) : IRequestHandler<GetRolesQuery, IReadOnlyCollection<RoleListItemDto>>
    {
        private const string PermissionClaimType = "Permission";
        private static readonly HashSet<string> ProtectedRoles = new(StringComparer.OrdinalIgnoreCase) { "Admin" };

        public async Task<IReadOnlyCollection<RoleListItemDto>> Handle(GetRolesQuery request, CancellationToken cancellationToken)
        {
            var roles = await roleManager.Roles
                .OrderBy(role => role.Name)
                .ToListAsync(cancellationToken);

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
