using CrawlScope.Domain.Modules.Crawling.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrawlScope.Persistence.Configurations.Crawling
{
    public class CrawlJobConfiguration : IEntityTypeConfiguration<CrawlJob>
    {
        public void Configure(EntityTypeBuilder<CrawlJob> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.TargetUrl)
                .IsRequired()
                .HasMaxLength(2048);

            builder.Property(x => x.MaxDepth).IsRequired();

            builder.Property(x => x.MaxPages).IsRequired();

            builder.Property(x => x.StayWithinDomain).IsRequired();

            builder.Property(x => x.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Property(x => x.CreatedBy) .IsRequired();

            builder.Property(x => x.ErrorMessage).HasMaxLength(2000);

            builder.HasMany(x => x.QueueItems)
                .WithOne(x => x.CrawlJob)
                .HasForeignKey(x => x.CrawlJobId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.CrawledPages)
                .WithOne(x => x.CrawlJob)
                .HasForeignKey(x => x.CrawlJobId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.CrawledPages)
                .WithOne(x => x.CrawlJob)
                .HasForeignKey(x => x.CrawlJobId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.Logs)
                .WithOne(x => x.CrawlJob)
                .HasForeignKey(x => x.CrawlJobId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.ExportFiles)
                .WithOne(x => x.CrawlJob)
                .HasForeignKey(x => x.CrawlJobId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
