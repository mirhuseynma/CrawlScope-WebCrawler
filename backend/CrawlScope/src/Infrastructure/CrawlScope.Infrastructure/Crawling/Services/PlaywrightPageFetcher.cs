using CrawlScope.Application.Abstractions.Crawling.Models;
using Microsoft.Playwright;
using System.Diagnostics;

namespace CrawlScope.Infrastructure.Crawling.Services
{
    public class PlaywrightPageFetcher : IPageFetcher
    {
        public async Task<PageFetchResult> FetchAsync(string url, CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                using var playwright = await Playwright.CreateAsync();
                
                // Launch headless chromium
                await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
                {
                    Headless = true,
                    Args = new[] { "--disable-gpu", "--disable-dev-shm-usage", "--no-sandbox" }
                });

                var context = await browser.NewContextAsync(new BrowserNewContextOptions
                {
                    UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36",
                    IgnoreHTTPSErrors = true
                });

                var page = await context.NewPageAsync();

                // Go to the URL and wait until there are no network connections for at least 500 ms.
                var response = await page.GotoAsync(url, new PageGotoOptions
                {
                    WaitUntil = WaitUntilState.NetworkIdle,
                    Timeout = 30000 // 30 seconds
                });

                if (response == null)
                {
                    throw new Exception("No response from the server.");
                }

                var content = await page.ContentAsync();
                var statusCode = response.Status;
                var contentType = response.Headers.TryGetValue("content-type", out var type) ? type : "text/html";

                stopwatch.Stop();

                return new PageFetchResult(
                    url,
                    statusCode,
                    content,
                    contentType,
                    stopwatch.ElapsedMilliseconds,
                    response.Ok,
                    null);
            }
            catch (Exception ex)
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
