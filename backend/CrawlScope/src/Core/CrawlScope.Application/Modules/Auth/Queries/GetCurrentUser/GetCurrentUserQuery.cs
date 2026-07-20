namespace CrawlScope.Application.Modules.Auth.Queries.GetCurrentUser
{
    public class GetCurrentUserQuery : IRequest<Result<CurrentUserDto>>
    {
        public string UserId { get; set; } = default!;
    }
}
