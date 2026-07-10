namespace CrawlScope.Application.Modules.Auth.DTOs
{
    public class UpdateUserRolesRequestDto
    {
        public IReadOnlyCollection<string> Roles { get; set; } = [];
    }
}
