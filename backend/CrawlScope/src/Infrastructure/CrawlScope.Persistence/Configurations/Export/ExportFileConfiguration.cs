using CrawlScope.Domain.Modules.Export.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrawlScope.Persistence.Configurations.Export
{
    public class ExportFileConfiguration : IEntityTypeConfiguration<ExportFile>
    {
        public void Configure(EntityTypeBuilder<ExportFile> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Format)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Property(x => x.FileName)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(x => x.FilePath)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(x => x.FileSizeBytes).IsRequired();

            builder.Property(x => x.CreatedByUserId).IsRequired();
        }
    }
}
