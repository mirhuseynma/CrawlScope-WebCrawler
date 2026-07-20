namespace CrawlScope.Application.Modules.Admin.Queries.Roles.GetPermissions
{
    public class GetPermissionsQueryHandler : IRequestHandler<GetPermissionsQuery, IReadOnlyCollection<PermissionDto>>
    {
        public Task<IReadOnlyCollection<PermissionDto>> Handle(GetPermissionsQuery request, CancellationToken cancellationToken)
        {
            IReadOnlyCollection<PermissionDto> permissions = [.. Permissions.All()
                .OrderBy(GetPermissionGroup)
                .ThenBy(GetPermissionName)
                .Select(permission => new PermissionDto
                {
                    Value = permission,
                    Group = GetPermissionGroup(permission),
                    Name = GetPermissionName(permission)
                })];

            return Task.FromResult(permissions);
        }

        private static string GetPermissionGroup(string permission)
        {
            var parts = permission.Split('.');
            return parts.Length >= 2 ? parts[1] : "General";
        }

        private static string GetPermissionName(string permission)
        {
            var parts = permission.Split('.');
            return parts.Length >= 3 ? parts[2] : permission;
        }
    }
}
