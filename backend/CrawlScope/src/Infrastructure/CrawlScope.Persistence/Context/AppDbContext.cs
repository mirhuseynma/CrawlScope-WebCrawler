using CrawlScope.Application.Abstractions.Persistence;
using CrawlScope.Domain.Modules.Auth.Models;
using CrawlScope.Domain.Modules.Crawling.Models;
using CrawlScope.Domain.Modules.Export.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CrawlScope.Persistence.Context
{
    public class AppDbContext(DbContextOptions<AppDbContext> options)
        : IdentityDbContext<AppUser, IdentityRole, string>(options), IAppDbContext
    {
        public DbSet<CrawlJob> CrawlJobs => Set<CrawlJob>();

        public DbSet<CrawlQueueItem> CrawlQueueItems => Set<CrawlQueueItem>();

        public DbSet<CrawledPage> CrawledPages => Set<CrawledPage>();

        public DbSet<CrawledLink> CrawledLinks => Set<CrawledLink>();

        public DbSet<CrawlLog> CrawlLogs => Set<CrawlLog>();

        public DbSet<ExportFile> ExportFiles => Set<ExportFile>();

        public DbSet<CrawlSchedule> CrawlSchedules => Set<CrawlSchedule>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }

    }
}
