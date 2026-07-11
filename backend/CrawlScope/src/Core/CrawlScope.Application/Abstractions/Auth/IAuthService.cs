

namespace CrawlScope.Application.Abstractions.Auth
{
    public interface IAuthService
    {
        Task<Result<AuthResponseDto>> RegisterAsync(RegisterRequestDto request);
        Task<Result<AuthResponseDto>> LoginAsync(LoginRequestDto request);
        Task<Result<CurrentUserDto>> GetCurrentUserAsync(string userId);
    }
}
