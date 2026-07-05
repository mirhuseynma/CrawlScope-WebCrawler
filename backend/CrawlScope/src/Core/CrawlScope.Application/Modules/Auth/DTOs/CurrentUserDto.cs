namespace CrawlScope.Application.Modules.Auth.DTOs
{
    public class CurrentUserDto
    {
        public string UserId { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? FullName { get; set; }
        public IReadOnlyCollection<string> Roles { get; set; } = [];
        public IReadOnlyCollection<string> Permissions { get; set; } = [];
    }
}
