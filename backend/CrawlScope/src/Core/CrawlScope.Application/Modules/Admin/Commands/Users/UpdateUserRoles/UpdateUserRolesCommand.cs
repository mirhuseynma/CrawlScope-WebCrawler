namespace CrawlScope.Application.Modules.Admin.Commands.Users.UpdateUserRoles
{
    public class UpdateUserRolesCommand : IRequest<UserDetailsDto>
    {
        public string UserId { get; set; } = default!;
        public UpdateUserRolesRequestDto Dto { get; set; } = default!;
    }
}
