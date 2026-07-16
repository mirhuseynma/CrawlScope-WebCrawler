using CrawlScope.Application.Abstractions.Crawling.Services;
using CrawlScope.Domain.Modules.Crawling.Enums;

namespace CrawlScope.Infrastructure.Crawling.Services
{
    public class UrlAnalyzerService(HttpClient httpClient) : IUrlAnalyzerService
    {
        public async Task<CrawlType> AnalyzeUrlAsync(string url, CancellationToken cancellationToken = default)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                // Add some standard headers to simulate a real browser slightly, 
                // but not too much, to see if they block basic HttpClients.
                request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
                request.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,*/*;q=0.8");

                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(TimeSpan.FromSeconds(5));

                using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cts.Token);

                // If it's a 403 Forbidden or 401 Unauthorized, it's likely bot protection
                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden || 
                    response.StatusCode == System.Net.HttpStatusCode.Unauthorized ||
                    response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable) // sometimes 503 is used by Cloudflare
                {
                    return CrawlType.Dynamic;
                }

                // If not success, default to Fast and let the normal crawl handle the error logging
                if (!response.IsSuccessStatusCode)
                {
                    return CrawlType.Fast;
                }

                // Read the start of the content to check for SPA indicators
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                
                // Very basic SPA detection: looking for root divs common in React/Vue/Angular
                // or lack of standard body content.
                if (content.Contains("id=\"root\"") || 
                    content.Contains("id=\"app\"") || 
                    content.Contains("<app-root></app-root>") ||
                    content.Contains("__NEXT_DATA__") ||
                    content.Contains("__NUXT__"))
                {
                    // It might be an SPA, but let's check if it actually has minimal HTML.
                    // SPAs usually have very few links in the raw HTML.
                    var aTagCount = content.Split("<a ").Length - 1;
                    if (aTagCount < 5) 
                    {
                        return CrawlType.Dynamic;
                    }
                }

                return CrawlType.Fast;
            }
            catch
            {
                // On exception (e.g. timeout, connection refused), fallback to Fast. 
                // Playwright would likely fail too if the site is completely down.
                return CrawlType.Fast;
            }
        }
    }
}
