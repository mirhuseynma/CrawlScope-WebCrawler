namespace CrawlScope.Application.Modules.Admin.Commands.Roles.DeleteRole
{
    public class DeleteRoleCommand : IRequest
    {
        public string RoleId { get; set; } = default!;
    }
}
