namespace CrawlScope.Application.Modules.Admin.Queries.Users.GetUsers
{
    public class GetUsersQuery : IRequest<PagedResult<UserListItemDto>>
    {
        public string? Search { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
