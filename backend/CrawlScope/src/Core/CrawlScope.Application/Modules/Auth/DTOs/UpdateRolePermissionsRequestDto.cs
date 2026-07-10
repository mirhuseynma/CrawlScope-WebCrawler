namespace CrawlScope.Application.Modules.Auth.DTOs
{
    public class UpdateRolePermissionsRequestDto
    {
        public IReadOnlyCollection<string> Permissions { get; set; } = [];
    }
}
