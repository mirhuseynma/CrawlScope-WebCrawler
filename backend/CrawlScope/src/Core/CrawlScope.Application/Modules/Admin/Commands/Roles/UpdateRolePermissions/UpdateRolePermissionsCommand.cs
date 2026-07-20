namespace CrawlScope.Application.Modules.Admin.Commands.Roles.UpdateRolePermissions
{
    public class UpdateRolePermissionsCommand : IRequest<RoleDetailsDto>
    {
        public string RoleId { get; set; } = default!;
        public UpdateRolePermissionsRequestDto Dto { get; set; } = default!;
    }
}
