namespace CrawlScope.Application.Modules.Auth.Commands.Login
{
    public class LoginCommandHandler(
        UserManager<AppUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IJwtService jwtService,
        IOptions<JwtSettings> jwtOptions) : IRequestHandler<LoginCommand, Result<AuthResponseDto>>
    {
        private readonly JwtSettings jwtSettings = jwtOptions.Value;

        public async Task<Result<AuthResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var user = await userManager.FindByEmailAsync(request.Dto.EmailOrUsername)
                ?? await userManager.FindByNameAsync(request.Dto.EmailOrUsername);
            
            if (user is null)
            {
                return Result<AuthResponseDto>.Failure("User not found.");
            }

            var isPasswordValid = await userManager.CheckPasswordAsync(user, request.Dto.Password);

            if (!isPasswordValid)
            {
                return Result<AuthResponseDto>.Failure("Invalid email/username or password.");
            }

            if (!await userManager.IsEmailConfirmedAsync(user))
            {
                return Result<AuthResponseDto>.Failure("Please confirm your email address before logging in.");
            }

            var authResponse = await CreateAuthResponseAsync(user);
            return Result<AuthResponseDto>.Success(authResponse);
        }

        private async Task<AuthResponseDto> CreateAuthResponseAsync(AppUser user)
        {
            var roles = await userManager.GetRolesAsync(user);
            var permissions = await GetPermissionsAsync(roles);
            var token = await jwtService.GenerateTokenAsync(user);
            var expiresAt = DateTime.UtcNow.AddMinutes(jwtSettings.ExpirationMinutes);

            return new AuthResponseDto
            {
                Token = token,
                ExpiresAt = expiresAt,
                ExpiresInSeconds = jwtSettings.ExpirationMinutes * 60,
                UserId = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                FullName = user.FullName,
                Roles = roles.ToArray(),
                Permissions = permissions.ToArray()
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
