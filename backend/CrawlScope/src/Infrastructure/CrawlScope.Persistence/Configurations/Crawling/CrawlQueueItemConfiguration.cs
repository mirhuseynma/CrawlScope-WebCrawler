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

            builder.Property(x => x.AnchorText).HasMaxLength(500);

            builder.Property(x => x.IsExternal).IsRequired();

            builder.Property(x => x.ErrorMessage).HasMaxLength(2000);

            builder.HasIndex(x => new { x.CrawlJobId, x.Url }).IsUnique();
        
        }
    }
}
