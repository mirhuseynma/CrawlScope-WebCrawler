namespace CrawlScope.Application.Modules.Auth.DTOs
{
    public class UserDetailsDto
    {
        public string Id { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? FullName { get; set; }
        public IReadOnlyCollection<string> Roles { get; set; } = [];
        public IReadOnlyCollection<string> Permissions { get; set; } = [];
        public bool IsSystemManaged { get; set; }
    }
}
