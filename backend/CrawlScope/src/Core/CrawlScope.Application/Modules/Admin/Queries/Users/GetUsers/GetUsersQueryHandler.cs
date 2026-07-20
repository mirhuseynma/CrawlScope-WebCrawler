namespace CrawlScope.Application.Modules.Admin.Queries.Users.GetUsers
{
    public class GetUsersQueryHandler(
        UserManager<AppUser> userManager) : IRequestHandler<GetUsersQuery, PagedResult<UserListItemDto>>
    {
        public async Task<PagedResult<UserListItemDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
        {
            var query = userManager.Users.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var trimmedSearch = request.Search.Trim();
                query = query.Where(user =>
                    (user.UserName != null && user.UserName.Contains(trimmedSearch))
                    || (user.Email != null && user.Email.Contains(trimmedSearch))
                    || (user.FullName != null && user.FullName.Contains(trimmedSearch)));
            }

            var pagedUsers = await PagedResult<AppUser>.CreateAsync(
                query.OrderBy(user => user.UserName),
                request.PageNumber,
                request.PageSize,
                cancellationToken);

            var items = new List<UserListItemDto>();

            foreach (var user in pagedUsers.Items)
            {
                var roles = await userManager.GetRolesAsync(user);
                items.Add(new UserListItemDto
                {
                    Id = user.Id,
                    UserName = user.UserName ?? string.Empty,
                    Email = user.Email ?? string.Empty,
                    FullName = user.FullName,
                    Roles = roles.ToArray(),
                    IsSystemManaged = await IsSystemManagedUserAsync(user)
                });
            }

            return new PagedResult<UserListItemDto>
            {
                Items = items,
                PageNumber = pagedUsers.PageNumber,
                PageSize = pagedUsers.PageSize,
                TotalCount = pagedUsers.TotalCount,
                TotalPages = pagedUsers.TotalPages
            };
        }

        private async Task<bool> IsSystemManagedUserAsync(AppUser user)
        {
            var claims = await userManager.GetClaimsAsync(user);
            return claims.Any(claim =>
                claim.Type == SystemClaims.SystemUser && claim.Value == SystemClaims.SeedAdmin);
        }
    }
}
