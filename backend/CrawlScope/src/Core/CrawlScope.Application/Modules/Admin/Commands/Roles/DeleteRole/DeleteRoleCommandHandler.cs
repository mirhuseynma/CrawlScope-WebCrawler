namespace CrawlScope.Application.Modules.Admin.Commands.Roles.DeleteRole
{
    public class DeleteRoleCommandHandler(
        RoleManager<IdentityRole> roleManager,
        UserManager<AppUser> userManager) : IRequestHandler<DeleteRoleCommand>
    {
        private static readonly HashSet<string> ProtectedRoles = new(StringComparer.OrdinalIgnoreCase) { "Admin", "User" };

        public async Task Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
        {
            var role = await FindRoleByIdAsync(request.RoleId);
            var roleName = role.Name ?? string.Empty;

            if (IsProtectedRole(roleName))
            {
                throw new BadRequestException($"System role '{roleName}' cannot be deleted.");
            }

            if (await CountUsersInRoleAsync(roleName) > 0)
            {
                throw new BadRequestException($"Role '{roleName}' is assigned to users and cannot be deleted.");
            }

            var result = await roleManager.DeleteAsync(role);

            if (!result.Succeeded)
            {
                ThrowIdentityErrors("Role deletion failed", result);
            }
        }

        private async Task<IdentityRole> FindRoleByIdAsync(string roleId)
        {
            return await roleManager.FindByIdAsync(roleId)
                ?? throw new NotFoundException("Role not found.");
        }

        private async Task<int> CountUsersInRoleAsync(string? roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
            {
                return 0;
            }

            var users = await userManager.GetUsersInRoleAsync(roleName);
            return users.Count;
        }

        private static bool IsProtectedRole(string? roleName)
        {
            return !string.IsNullOrWhiteSpace(roleName) && ProtectedRoles.Contains(roleName);
        }

        private static void ThrowIdentityErrors(string message, IdentityResult result)
        {
            var errors = string.Join(", ", result.Errors.Select(error => error.Description));
            throw new BadRequestException($"{message}: {errors}");
        }
    }
}
