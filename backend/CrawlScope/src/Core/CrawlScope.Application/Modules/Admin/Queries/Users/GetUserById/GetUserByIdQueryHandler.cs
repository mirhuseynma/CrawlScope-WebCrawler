namespace CrawlScope.Application.Modules.Admin.Queries.Users.GetUserById
{
    public class GetUserByIdQueryHandler(
        UserManager<AppUser> userManager,
        RoleManager<IdentityRole> roleManager) : IRequestHandler<GetUserByIdQuery, UserDetailsDto>
    {
        private const string PermissionClaimType = "Permission";

        public async Task<UserDetailsDto> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            var user = await FindUserByIdAsync(request.UserId);
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
    }
}
