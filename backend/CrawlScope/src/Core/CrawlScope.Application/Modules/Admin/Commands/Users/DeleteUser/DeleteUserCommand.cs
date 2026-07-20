namespace CrawlScope.Application.Modules.Admin.Commands.Users.DeleteUser
{
    public class DeleteUserCommand : IRequest
    {
        public string UserId { get; set; } = default!;
    }
}
