namespace CrawlScope.Application.Modules.Auth.DTOs
{
    public class RoleDetailsDto
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public IReadOnlyCollection<string> Permissions { get; set; } = [];
        public int UserCount { get; set; }
        public bool IsSystemManaged { get; set; }
    }
}
