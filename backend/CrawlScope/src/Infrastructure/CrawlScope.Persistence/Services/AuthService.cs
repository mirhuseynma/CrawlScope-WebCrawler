
namespace CrawlScope.Persistence.Services
{
    public class AuthService(
        IJwtService jwtService,
        UserManager<AppUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IOptions<JwtSettings> jwtOptions,
        IEmailService emailService) : IAuthService
    {
        private readonly JwtSettings jwtSettings = jwtOptions.Value;

        public async Task<Result<AuthResponseDto>> RegisterAsync(RegisterRequestDto request, string clientBaseUrl)
        {
            var user = new AppUser
            {
                UserName = request.UserName,
                Email = request.Email,
                FullName = request.FullName
            };

            var result = await userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(error => error.Description));
                return Result<AuthResponseDto>.Failure($"User registration failed: {errors}");
            }

            await userManager.AddToRoleAsync(user, "User");
            
            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var confirmLink = $"{clientBaseUrl}/confirm-email?userId={user.Id}&token={encodedToken}";

            var body = $@"
                <h3>Confirm Your Account</h3>
                <p>Hello {user.FullName},</p>
                <p>Thank you for registering. Please click the link below to verify your email address:</p>
                <p><a href='{confirmLink}'>Confirm Email</a></p>";

            //await emailService.SendEmailAsync(user.Email!, "Confirm Email - CrawlScope", body);
            user.EmailConfirmed = true;

            // Return a dummy/empty token for registration, client won't log in immediately
            return Result<AuthResponseDto>.Success(new AuthResponseDto 
            { 
                Token = "",
                UserId = user.Id,
                Email = user.Email!,
                UserName = user.UserName!,
                FullName = user.FullName
            });
        }

        public async Task<Result<AuthResponseDto>> LoginAsync(LoginRequestDto request)
        {
            var user = await userManager.FindByEmailAsync(request.EmailOrUsername)
                ?? await userManager.FindByNameAsync(request.EmailOrUsername);
            
            if (user is null)
            {
                return Result<AuthResponseDto>.Failure("User not found.");
            }

            var isPasswordValid = await userManager.CheckPasswordAsync(user, request.Password);

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

        public async Task<Result<CurrentUserDto>> GetCurrentUserAsync(string userId)
        {
            var user = await userManager.FindByIdAsync(userId);
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

        public async Task<Result<bool>> ForgotPasswordAsync(ForgotPasswordRequestDto request, string clientBaseUrl)
        {
            var user = await userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return Result<bool>.Success(true);
            }

            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var resetLink = $"{clientBaseUrl}/reset-password?email={request.Email}&token={encodedToken}";

            var body = $@"
                <h3>Reset Your Password</h3>
                <p>Hello {user.FullName},</p>
                <p>You requested a password reset. Click the link below to reset your password:</p>
                <p><a href='{resetLink}'>Reset Password</a></p>
                <p>If you didn't request this, you can safely ignore this email.</p>";

            await emailService.SendEmailAsync(user.Email!, "Reset Password - CrawlScope", body);

            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> ResetPasswordAsync(ResetPasswordRequestDto request)
        {
            var user = await userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return Result<bool>.Failure("Invalid request.");
            }

            var decodedTokenBytes = WebEncoders.Base64UrlDecode(request.Token);
            var decodedToken = Encoding.UTF8.GetString(decodedTokenBytes);

            var result = await userManager.ResetPasswordAsync(user, decodedToken, request.NewPassword);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return Result<bool>.Failure($"Password reset failed: {errors}");
            }

            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> ConfirmEmailAsync(string userId, string token)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Result<bool>.Failure("User not found.");
            }

            var decodedTokenBytes = WebEncoders.Base64UrlDecode(token);
            var decodedToken = Encoding.UTF8.GetString(decodedTokenBytes);

            var result = await userManager.ConfirmEmailAsync(user, decodedToken);
            if (!result.Succeeded)
            {
                return Result<bool>.Failure("Email confirmation failed.");
            }

            return Result<bool>.Success(true);
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
