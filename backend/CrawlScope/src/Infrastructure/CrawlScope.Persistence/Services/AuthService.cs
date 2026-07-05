using System.Security.Claims;
using CrawlScope.Application.Abstractions.Auth;
using CrawlScope.Application.Common.Exceptions;
using CrawlScope.Application.Common.Settings;
using CrawlScope.Application.Modules.Auth.DTOs;
using CrawlScope.Domain.Modules.Auth.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace CrawlScope.Persistence.Services
{
    public class AuthService(
        IJwtService jwtService,
        UserManager<AppUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IOptions<JwtSettings> jwtOptions) : IAuthService
    {
        private readonly JwtSettings jwtSettings = jwtOptions.Value;

        public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
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
                throw new BadRequestException($"User registration failed: {errors}");
            }

            await userManager.AddToRoleAsync(user, "User");
            return await CreateAuthResponseAsync(user);
        }

        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
        {
            var user = await userManager.FindByEmailAsync(request.EmailOrUsername)
                ?? await userManager.FindByNameAsync(request.EmailOrUsername)
                ?? throw new BadRequestException("Invalid email/username or password.");

            var isPasswordValid = await userManager.CheckPasswordAsync(user, request.Password);

            if (!isPasswordValid)
            {
                throw new BadRequestException("Invalid email/username or password.");
            }

            return await CreateAuthResponseAsync(user);
        }

        public async Task<CurrentUserDto> GetCurrentUserAsync(string userId)
        {
            var user = await userManager.FindByIdAsync(userId)
                ?? throw new NotFoundException("User not found.");

            var roles = await userManager.GetRolesAsync(user);
            var permissions = await GetPermissionsAsync(roles);

            return new CurrentUserDto
            {
                UserId = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                FullName = user.FullName,
                Roles = roles.ToArray(),
                Permissions = permissions.ToArray()
            };
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
