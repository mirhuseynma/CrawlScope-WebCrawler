namespace CrawlScope.Application.Modules.Admin.Commands.Roles.CreateRole
{
    public class CreateRoleCommand : IRequest<RoleDetailsDto>
    {
        public CreateRoleRequestDto Dto { get; set; } = default!;
    }
}
