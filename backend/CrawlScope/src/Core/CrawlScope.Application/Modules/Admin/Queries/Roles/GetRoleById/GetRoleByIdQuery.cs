namespace CrawlScope.Application.Modules.Admin.Queries.Roles.GetRoleById
{
    public class GetRoleByIdQuery : IRequest<RoleDetailsDto>
    {
        public string RoleId { get; set; } = default!;
    }
}
