namespace CrawlScope.Application.Modules.Auth.DTOs
{
    public class LoginRequestDto
    {
        public string EmailOrUsername { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}
