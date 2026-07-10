namespace CrawlScope.Application.Modules.Auth.DTOs
{
    public class CreateRoleRequestDto
    {
        public string Name { get; set; } = null!;
        public IReadOnlyCollection<string> Permissions { get; set; } = [];
    }
}
