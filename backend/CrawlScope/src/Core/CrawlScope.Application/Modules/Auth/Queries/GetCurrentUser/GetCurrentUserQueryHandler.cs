namespace CrawlScope.Application.Modules.Auth.Queries.GetCurrentUser
{
    public class GetCurrentUserQueryHandler(
        UserManager<AppUser> userManager,
        RoleManager<IdentityRole> roleManager) : IRequestHandler<GetCurrentUserQuery, Result<CurrentUserDto>>
    {
        public async Task<Result<CurrentUserDto>> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
        {
            var user = await userManager.FindByIdAsync(request.UserId);
            if (user is null)
            {
                return Result<CurrentUserDto>.Failure("User not found.");
            }

            var roles = await userManager.GetRolesAsync(user);
            var permissions = await GetPermissionsAsync(roles);

            var currentUser = new CurrentUserDto
            {
                UserId = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                FullName = user.FullName,
                Roles = roles.ToArray(),
                Permissions = permissions.ToArray()
            };
            return Result<CurrentUserDto>.Success(currentUser);
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

                var roleClaims = await roleManager.GetClaimsAsync(role);

                foreach (var claim in roleClaims.Where(claim => claim.Type == "Permission"))
                {
                    permissions.Add(claim.Value);
                }
            }

            return permissions.ToArray();
        }
    }
}
