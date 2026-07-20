namespace CrawlScope.Application.Modules.Admin.Commands.Users.UpdateUser
{
    public class UpdateUserCommand : IRequest<UserDetailsDto>
    {
        public string UserId { get; set; } = default!;
        public UpdateUserRequestDto Dto { get; set; } = default!;
    }
}
