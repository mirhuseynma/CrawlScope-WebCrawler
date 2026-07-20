namespace CrawlScope.Application.Modules.Admin.Queries.Users.GetUserById
{
    public class GetUserByIdQuery : IRequest<UserDetailsDto>
    {
        public string UserId { get; set; } = default!;
    }
}
