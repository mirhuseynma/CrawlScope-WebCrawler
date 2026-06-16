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
    public class CrawlLogConfiguration : IEntityTypeConfiguration<CrawlLog>
    {
        public void Configure(EntityTypeBuilder<CrawlLog> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Level)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Property(x => x.Message)
                .IsRequired()
                .HasMaxLength(2000);
        }
    }
}
