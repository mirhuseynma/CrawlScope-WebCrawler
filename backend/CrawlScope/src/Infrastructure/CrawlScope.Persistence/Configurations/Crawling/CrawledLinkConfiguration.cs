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
    public class CrawledLinkConfiguration : IEntityTypeConfiguration<CrawledLink>
    {
        public void Configure(EntityTypeBuilder<CrawledLink> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.SourceUrl)
                .IsRequired()
                .HasMaxLength(2048);

            builder.Property(x => x.TargetUrl)
                .IsRequired()
                .HasMaxLength (2048);

            builder.Property(x => x.AnchorText)
                .HasMaxLength(512);

            builder.Property(x => x.IsExternal)
                .IsRequired();

            builder.Property(x => x.DepthLevel)
                .IsRequired();

            builder.HasIndex(x => new { x.CrawledPageId, x.TargetUrl });



        }
    }
}
