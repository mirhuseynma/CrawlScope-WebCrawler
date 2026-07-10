namespace CrawlScope.Application.Modules.Auth.DTOs
{
    public class UpdateUserRequestDto
    {
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? FullName { get; set; }
    }
}
