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
