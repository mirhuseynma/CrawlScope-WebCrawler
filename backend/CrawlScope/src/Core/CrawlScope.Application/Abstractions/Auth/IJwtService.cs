
namespace CrawlScope.Application.Abstractions.Auth
{
    public interface IJwtService
    {
        Task<string> GenerateTokenAsync(AppUser user);
    }
}
