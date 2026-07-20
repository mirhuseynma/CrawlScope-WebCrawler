namespace CrawlScope.Application.Modules.Admin.Commands.Users.DeleteUser
{
    public class DeleteUserCommandHandler(
        UserManager<AppUser> userManager) : IRequestHandler<DeleteUserCommand>
    {
        private const string AdminRoleName = "Admin";

        public async Task Handle(DeleteUserCommand request, CancellationToken cancellationToken)
        {
            var user = await FindUserByIdAsync(request.UserId);

            if (await IsSystemManagedUserAsync(user))
            {
                throw new BadRequestException("System admin user cannot be deleted.");
            }

            if (await userManager.IsInRoleAsync(user, AdminRoleName))
            {
                await EnsureAnotherAdminExistsAsync(user.Id);
            }

            var result = await userManager.DeleteAsync(user);

            if (!result.Succeeded)
            {
                ThrowIdentityErrors("User deletion failed", result);
            }
        }

        private async Task<AppUser> FindUserByIdAsync(string userId)
        {
            return await userManager.FindByIdAsync(userId)
                ?? throw new NotFoundException("User not found.");
        }

        private async Task<bool> IsSystemManagedUserAsync(AppUser user)
        {
            var claims = await userManager.GetClaimsAsync(user);
            return claims.Any(claim =>
                claim.Type == SystemClaims.SystemUser && claim.Value == SystemClaims.SeedAdmin);
        }

        private async Task EnsureAnotherAdminExistsAsync(string currentUserId)
        {
            var adminUsers = await userManager.GetUsersInRoleAsync(AdminRoleName);
            var hasAnotherAdmin = adminUsers.Any(user => user.Id != currentUserId);

            if (!hasAnotherAdmin)
            {
                throw new BadRequestException("At least one admin user must remain.");
            }
        }

        private static void ThrowIdentityErrors(string message, IdentityResult result)
        {
            var errors = string.Join(", ", result.Errors.Select(error => error.Description));
            throw new BadRequestException($"{message}: {errors}");
        }
    }
}
