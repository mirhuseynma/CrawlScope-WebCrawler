
namespace CrawlScope.Application.Tests.Auth;

public class PermissionAuthorizationTests
{
    [Fact]
    public async Task HandleAsync_WhenUserHasRequiredPermission_ShouldSucceed()
    {
        var requirement = new PermissionRequirement(Permissions.Admin.Access);
        var user = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim("Permission", Permissions.Admin.Access)],
            authenticationType: "Test"));
        var context = new AuthorizationHandlerContext([requirement], user, resource: null);
        var handler = new PermissionAuthorizationHandler();

        await handler.HandleAsync(context);

        Assert.True(context.HasSucceeded);
    }

    [Fact]
    public async Task HandleAsync_WhenUserDoesNotHaveRequiredPermission_ShouldNotSucceed()
    {
        var requirement = new PermissionRequirement(Permissions.Admin.Access);
        var user = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim("Permission", Permissions.CrawlJobs.View)],
            authenticationType: "Test"));
        var context = new AuthorizationHandlerContext([requirement], user, resource: null);
        var handler = new PermissionAuthorizationHandler();

        await handler.HandleAsync(context);

        Assert.False(context.HasSucceeded);
    }
}
