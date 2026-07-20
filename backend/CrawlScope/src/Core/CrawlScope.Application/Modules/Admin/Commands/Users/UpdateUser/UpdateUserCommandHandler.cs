namespace CrawlScope.Application.Modules.Admin.Commands.Users.UpdateUser
{
    public class UpdateUserCommandHandler(
        UserManager<AppUser> userManager,
        RoleManager<IdentityRole> roleManager) : IRequestHandler<UpdateUserCommand, UserDetailsDto>
    {
        private const string PermissionClaimType = "Permission";

        public async Task<UserDetailsDto> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            var user = await FindUserByIdAsync(request.UserId);

            if (await IsSystemManagedUserAsync(user))
            {
                throw new BadRequestException("System admin user can only be managed from seed configuration.");
            }

            var userName = NormalizeRequired(request.Dto.UserName, "User name");
            var email = NormalizeRequired(request.Dto.Email, "Email");

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

            user.FullName = string.IsNullOrWhiteSpace(request.Dto.FullName)
                ? null
                : request.Dto.FullName.Trim();

            var updateResult = await userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
            {
                ThrowIdentityErrors("User update failed", updateResult);
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
