namespace CrawlScope.Api.Common.Http
{
    public class ApiErrorResponse
    {
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public object? Errors { get; set; }
        public string? TraceId { get; set; }
    }
}
