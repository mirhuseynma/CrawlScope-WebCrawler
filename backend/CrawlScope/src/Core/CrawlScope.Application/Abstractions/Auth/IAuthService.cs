

namespace CrawlScope.Application.Abstractions.Auth
{
    public interface IAuthService
    {
        Task<Result<AuthResponseDto>> RegisterAsync(RegisterRequestDto request, string clientBaseUrl);
        Task<Result<AuthResponseDto>> LoginAsync(LoginRequestDto request);
        Task<Result<CurrentUserDto>> GetCurrentUserAsync(string userId);
        Task<Result<bool>> ForgotPasswordAsync(ForgotPasswordRequestDto request, string clientBaseUrl);
        Task<Result<bool>> ResetPasswordAsync(ResetPasswordRequestDto request);
        Task<Result<bool>> ConfirmEmailAsync(string userId, string token);
    }
}
