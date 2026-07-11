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
