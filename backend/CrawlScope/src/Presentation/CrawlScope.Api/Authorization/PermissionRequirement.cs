using Microsoft.AspNetCore.Authorization;

namespace CrawlScope.Api.Authorization
{
    public class PermissionRequirement(string permission) : IAuthorizationRequirement
    {
        public string Permission { get; } = permission;
    }
}
