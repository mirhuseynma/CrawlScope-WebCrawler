namespace CrawlScope.Application.Modules.Admin.DTOs
{
    public class AdminOverviewTotalsDto
    {
        public int TotalJobs { get; set; }
        public int PendingJobs { get; set; }
        public int InProgressJobs { get; set; }
        public int CompletedJobs { get; set; }
        public int FailedJobs { get; set; }
        public int CanceledJobs { get; set; }
        public int ImportantJobs { get; set; }
        public int TotalPages { get; set; }
        public int FailedPages { get; set; }
        public int TotalExports { get; set; }
        public long TotalExportSizeBytes { get; set; }
    }
}
