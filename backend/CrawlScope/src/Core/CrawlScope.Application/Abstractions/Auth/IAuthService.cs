using CrawlScope.Application.Modules.Auth.DTOs;

namespace CrawlScope.Application.Abstractions.Auth
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request);
        Task<AuthResponseDto> LoginAsync(LoginRequestDto request);
        Task<CurrentUserDto> GetCurrentUserAsync(string userId);
    }
}
