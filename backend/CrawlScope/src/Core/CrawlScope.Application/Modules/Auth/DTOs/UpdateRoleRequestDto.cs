namespace CrawlScope.Application.Modules.Auth.DTOs
{
    public class UpdateRoleRequestDto
    {
        public string Name { get; set; } = string.Empty;
        public IReadOnlyCollection<string> Permissions { get; set; } = [];
    }
}
