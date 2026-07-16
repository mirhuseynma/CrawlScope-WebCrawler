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
                    ViewportSize = new ViewportSize { Width = 1920, Height = 1080 },
                    JavaScriptEnabled = true,
                    BypassCSP = true,
                    IgnoreHTTPSErrors = true,
                    ExtraHTTPHeaders = new Dictionary<string, string>
                    {
                        { "Accept-Language", "en-US,en;q=0.9" },
                        { "Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,*/*;q=0.8" }
                    }
                });

                // Anti-bot stealth script to remove webdriver flag
                await context.AddInitScriptAsync(script: @"
                    Object.defineProperty(navigator, 'webdriver', { get: () => undefined });
                    window.chrome = { runtime: {} };
                ");

                var page = await context.NewPageAsync();

                // Go to the URL and wait for the 'load' event (base HTML and resources loaded)
                var response = await page.GotoAsync(url, new PageGotoOptions
                {
                    WaitUntil = WaitUntilState.Load,
                    Timeout = 30000 // 30 seconds
                });

                try
                {
                    // Try to wait for the network to become completely idle so JavaScript can finish rendering.
                    // If it's a normal site, this finishes in milliseconds.
                    // If it's an SPA with non-stop background polling (like your dashboard), it will hit the 7-second timeout.
                    await page.WaitForLoadStateAsync(LoadState.NetworkIdle, new PageWaitForLoadStateOptions { Timeout = 7000 });
                }
                catch (TimeoutException)
                {
                    // Ignore the timeout. The page has been rendering for 7 seconds already,
                    // which is more than enough for 99% of React/Vue apps to show their content.
                }

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
