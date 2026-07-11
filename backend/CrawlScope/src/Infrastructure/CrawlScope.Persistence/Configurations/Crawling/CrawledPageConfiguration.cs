namespace CrawlScope.Persistence.Configurations.Crawling
{
    public class CrawledPageConfiguration : IEntityTypeConfiguration<CrawledPage>
    {
        public void Configure(EntityTypeBuilder<CrawledPage> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Url)
                .IsRequired()
                .HasMaxLength(2048);

            builder.Property(x => x.Title).HasMaxLength(500);

            builder.Property(x => x.Content);

            builder.Property(x => x.StatusCode);

            builder.Property(x => x.DepthLevel)
                .IsRequired();

            builder.HasIndex(x => new { x.CrawlJobId, x.Url })
                .IsUnique();

            builder.HasMany(x => x.Links)
                .WithOne(x => x.CrawledPage)
                .HasForeignKey(x => x.CrawledPageId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
