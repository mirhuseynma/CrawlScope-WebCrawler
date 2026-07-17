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
                request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/126.0.0.0 Safari/537.36");
                request.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,*/*;q=0.8");
                request.Headers.Add("Accept-Language", "en-US,en;q=0.9");

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
                if (content.Contains("id=\"root\"", StringComparison.OrdinalIgnoreCase) ||
                    content.Contains("id=\"app\"", StringComparison.OrdinalIgnoreCase) ||
                    content.Contains("<app-root", StringComparison.OrdinalIgnoreCase) ||
                    content.Contains("__NEXT_DATA__", StringComparison.OrdinalIgnoreCase) ||
                    content.Contains("__NUXT__", StringComparison.OrdinalIgnoreCase) ||
                    content.Contains("/assets/index-", StringComparison.OrdinalIgnoreCase) ||
                    content.Contains("type=\"module\"", StringComparison.OrdinalIgnoreCase))
                {
                    // It might be an SPA, but let's check if it actually has minimal HTML.
                    // SPAs usually have very few links in the raw HTML.
                    var aTagCount = content.Split("<a ", StringSplitOptions.None).Length - 1;
                    var bodyText = System.Text.RegularExpressions.Regex.Replace(content, "<script[\\s\\S]*?</script>|<style[\\s\\S]*?</style>|<[^>]+>", " ");
                    if (aTagCount < 5 || bodyText.Trim().Length < 300)
                    {
                        return CrawlType.Dynamic;
                    }
                }

                return CrawlType.Fast;
            }
            catch
            {
                // On exception (e.g. timeout, connection dropped by anti-bot firewall), fallback to Dynamic.
                // Many modern firewalls simply stall basic HTTP requests indefinitely.
                return CrawlType.Dynamic;
            }
        }
    }
}
