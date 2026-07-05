using Microsoft.AspNetCore.Identity;

namespace CrawlScope.Domain.Modules.Auth.Models
{
    public class AppUser : IdentityUser
    {
        public string? FullName { get; set; }
    }
}
