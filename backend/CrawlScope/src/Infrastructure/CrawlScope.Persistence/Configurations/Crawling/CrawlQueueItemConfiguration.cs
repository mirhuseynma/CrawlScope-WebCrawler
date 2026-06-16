using CrawlScope.Domain.Modules.Crawling.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrawlScope.Persistence.Configurations.Crawling
{
    public class CrawlQueueItemConfiguration : IEntityTypeConfiguration<CrawlQueueItem>
    {
        public void Configure(EntityTypeBuilder<CrawlQueueItem> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Url)
                .IsRequired()
                .HasMaxLength(2048);

            builder.Property(x => x.DepthLevel).IsRequired();

            builder.Property(x => x.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Property(x => x.DiscoveredFromUrl).HasMaxLength(2048);

            builder.Property(x => x.ErrorMessage).HasMaxLength(2000);

            builder.HasIndex(x => new { x.CrawlJobId, x.Url }).IsUnique();
        
        }
    }
}
