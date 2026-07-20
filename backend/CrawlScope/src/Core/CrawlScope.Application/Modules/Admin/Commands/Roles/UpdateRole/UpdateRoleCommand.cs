namespace CrawlScope.Application.Modules.Admin.Commands.Roles.UpdateRole
{
    public class UpdateRoleCommand : IRequest<RoleDetailsDto>
    {
        public string RoleId { get; set; } = default!;
        public UpdateRoleRequestDto Dto { get; set; } = default!;
    }
}
