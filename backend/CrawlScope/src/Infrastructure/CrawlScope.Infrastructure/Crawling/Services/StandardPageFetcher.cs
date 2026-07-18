
namespace CrawlScope.Infrastructure.Crawling.Services
{
    public class StandardPageFetcher(HttpClient httpClient) : IPageFetcher
    {
        public async Task<PageFetchResult> FetchAsync(string url, CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                using var response = await httpClient.GetAsync(
                    url,
                    HttpCompletionOption.ResponseHeadersRead,
                    cancellationToken);

                var contentType = response.Content.Headers.ContentType?.MediaType;
                var content = await response.Content.ReadAsStringAsync(cancellationToken);

                stopwatch.Stop();

                return new PageFetchResult(
                    response.RequestMessage?.RequestUri?.ToString() ?? url,
                    (int)response.StatusCode,
                    content,
                    contentType,
                    stopwatch.ElapsedMilliseconds,
                    response.IsSuccessStatusCode,
                    null);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or OperationCanceledException)
            {
                stopwatch.Stop();

                return new PageFetchResult(
                    url,
                    null,
                    null,
                    null,
                    stopwatch.ElapsedMilliseconds,
                    false,
                    ex.Message);
            }
        }
    }
}
