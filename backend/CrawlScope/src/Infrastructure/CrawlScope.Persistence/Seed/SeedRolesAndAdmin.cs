
namespace CrawlScope.Persistence.Seed
{
    public static class SeedRolesAndAdmin
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<AppUser>>();
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();

            await EnsureRoleAsync(roleManager, "Admin", Permissions.All());
            await EnsureRoleAsync(roleManager, "User", Permissions.UserDefaults());

            var adminEmail = "admin@crawlscope.local";
            var admin = await userManager.FindByEmailAsync(adminEmail);

            if (admin is null)
            {
                admin = new AppUser
                {
                    UserName = "admin",
                    Email = adminEmail,
                    FullName = "CrawlScope Admin",
                    EmailConfirmed = true
                };

                var adminPassword = configuration["SeedSettings:AdminPassword"];
                if (string.IsNullOrWhiteSpace(adminPassword))
                {
                    throw new InvalidOperationException("SeedSettings:AdminPassword is not configured. Please set it in appsettings.json, User Secrets, or Environment Variables.");
                }

                var result = await userManager.CreateAsync(admin, adminPassword);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(error => error.Description));
                    throw new InvalidOperationException($"Admin seed failed: {errors}");
                }
            }
                await userManager.AddToRoleAsync(admin, "Admin");
                var adminClaims = await userManager.GetClaimsAsync(admin);
                var hasSystemAdminClaim = adminClaims.Any(claim =>
                    claim.Type == SystemClaims.SystemUser && claim.Value == SystemClaims.SeedAdmin);

                if (!hasSystemAdminClaim)
                {
                    await userManager.AddClaimAsync(admin, new Claim(SystemClaims.SystemUser, SystemClaims.SeedAdmin));
                }
        }

        private static async Task EnsureRoleAsync(
            RoleManager<IdentityRole> roleManager,
            string roleName,
            IEnumerable<string> permissions)
        {
            var role = await roleManager.FindByNameAsync(roleName);

            if (role is null)
            {
                role = new IdentityRole(roleName);
                var createResult = await roleManager.CreateAsync(role);

                if (!createResult.Succeeded)
                {
                    var errors = string.Join(", ", createResult.Errors.Select(error => error.Description));
                    throw new InvalidOperationException($"Role seed failed for {roleName}: {errors}");
                }
            }

            var existingClaims = await roleManager.GetClaimsAsync(role);

            foreach (var permission in permissions)
            {
                var hasPermission = existingClaims.Any(claim => claim.Type == "Permission" && claim.Value == permission);

                if (!hasPermission)
                {
                    await roleManager.AddClaimAsync(role, new Claim("Permission", permission));
                }
            }
        }
    }
}
