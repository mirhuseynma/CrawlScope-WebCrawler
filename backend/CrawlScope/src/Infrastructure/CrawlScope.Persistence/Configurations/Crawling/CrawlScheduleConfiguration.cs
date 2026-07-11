namespace CrawlScope.Persistence.Configurations.Crawling
{
    public class CrawlScheduleConfiguration : IEntityTypeConfiguration<CrawlSchedule>
    {
        public void Configure(EntityTypeBuilder<CrawlSchedule> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.TargetUrl)
                .IsRequired()
                .HasMaxLength(2048);

            builder.Property(x => x.MaxDepth).IsRequired();
            builder.Property(x => x.MaxPages).IsRequired();
            builder.Property(x => x.StayWithinDomain).IsRequired();
            builder.Property(x => x.IntervalMinutes).IsRequired();
            builder.Property(x => x.IsEnabled).IsRequired();
            builder.Property(x => x.CreatedBy).IsRequired();

            builder.HasIndex(x => x.NextRunAt);
            builder.HasIndex(x => x.IsEnabled);
        }
    }
}
