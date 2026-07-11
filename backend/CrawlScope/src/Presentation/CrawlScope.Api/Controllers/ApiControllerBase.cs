using System.Security.Claims;

namespace CrawlScope.Api.Controllers
{
    public abstract class ApiControllerBase : ControllerBase
    {
        protected string CurrentUserId =>
            TryGetCurrentUserId()
            ?? throw new UnauthorizedAccessException("Authenticated user id was not found.");

        protected bool CanAccessAllUsers =>
            User.Claims.Any(claim => claim.Type == "Permission" && claim.Value == Permissions.Admin.Access);

        protected string? TryGetCurrentUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return string.IsNullOrWhiteSpace(userId) ? null : userId;
        }
    }
}
