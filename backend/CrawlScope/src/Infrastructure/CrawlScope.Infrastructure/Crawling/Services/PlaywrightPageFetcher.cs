using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace CrawlScope.Infrastructure.Crawling.Services
{
    public class PlaywrightPageFetcher(ILogger<PlaywrightPageFetcher> logger) : IPageFetcher
    {
        public async Task<PageFetchResult> FetchAsync(string url, CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();
            var disposalSw = new Stopwatch();
            
            try
            {
                var stepSw = Stopwatch.StartNew();
                logger.LogInformation("Playwright.CreateAsync started for URL: {Url}", url);
                using var playwright = await Playwright.CreateAsync();
                logger.LogInformation("Playwright.CreateAsync completed in {Elapsed}ms", stepSw.ElapsedMilliseconds);
                
                stepSw.Restart();
                logger.LogInformation("Chromium.LaunchAsync started");
                // Launch headless chromium (with Docker-safe flags)
                await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
                {
                    Headless = true,
                    Args = new[] { "--disable-gpu", "--disable-dev-shm-usage", "--no-sandbox", "--disable-blink-features=AutomationControlled" }
                });
                logger.LogInformation("Chromium.LaunchAsync completed in {Elapsed}ms", stepSw.ElapsedMilliseconds);

                stepSw.Restart();
                logger.LogInformation("NewContextAsync started");
                var context = await browser.NewContextAsync(new BrowserNewContextOptions
                {
                    UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/126.0.0.0 Safari/537.36",
                    ViewportSize = new ViewportSize { Width = 1920, Height = 1080 },
                    JavaScriptEnabled = true,
                    BypassCSP = true,
                    IgnoreHTTPSErrors = true,
                    Locale = "en-US",
                    TimezoneId = "Asia/Baku",
                    ExtraHTTPHeaders = new Dictionary<string, string>
                    {
                        { "Accept-Language", "en-US,en;q=0.9" },
                        { "Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,*/*;q=0.8" }
                    }
                });
                logger.LogInformation("NewContextAsync completed in {Elapsed}ms", stepSw.ElapsedMilliseconds);

                stepSw.Restart();
                // Anti-bot stealth script to remove webdriver flag
                await context.AddInitScriptAsync(script: @"
                    Object.defineProperty(navigator, 'webdriver', { get: () => undefined });
                    window.chrome = { runtime: {} };
                ");

                logger.LogInformation("NewPageAsync started");
                var page = await context.NewPageAsync();
                logger.LogInformation("NewPageAsync completed in {Elapsed}ms", stepSw.ElapsedMilliseconds);

                stepSw.Restart();
                logger.LogInformation("GotoAsync started for URL: {Url}", url);
                // Go to the URL and wait for the 'load' event (base HTML and resources loaded)
                var response = await page.GotoAsync(url, new PageGotoOptions
                {
                    WaitUntil = WaitUntilState.Load,
                    Timeout = 30000 // 30 seconds
                });
                logger.LogInformation("GotoAsync completed in {Elapsed}ms", stepSw.ElapsedMilliseconds);

                try
                {
                    stepSw.Restart();
                    logger.LogInformation("WaitForLoadStateAsync(NetworkIdle) started");
                    // Try to wait for the network to become completely idle so JavaScript can finish rendering.
                    await page.WaitForLoadStateAsync(LoadState.NetworkIdle, new PageWaitForLoadStateOptions { Timeout = 7000 });
                    logger.LogInformation("WaitForLoadStateAsync completed in {Elapsed}ms", stepSw.ElapsedMilliseconds);
                }
                catch (TimeoutException)
                {
                    logger.LogWarning("WaitForLoadStateAsync timed out after 7s for URL: {Url}. Continuing anyway.", url);
                    // Ignore the timeout.
                }

                if (response == null)
                {
                    throw new Exception("No response from the server.");
                }

                stepSw.Restart();
                logger.LogInformation("ContentAsync started");
                var content = await page.ContentAsync();
                logger.LogInformation("ContentAsync completed in {Elapsed}ms", stepSw.ElapsedMilliseconds);
                
                var statusCode = response.Status;
                var contentType = response.Headers.TryGetValue("content-type", out var type) ? type : "text/html";

                stopwatch.Stop();
                disposalSw.Start();

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
                logger.LogError(ex, "Exception caught during Playwright FetchAsync for URL: {Url}", url);
                stopwatch.Stop();
                disposalSw.Start();

                return new PageFetchResult(
                    url,
                    null,
                    null,
                    null,
                    stopwatch.ElapsedMilliseconds,
                    false,
                    ex.Message);
            }
            finally
            {
                disposalSw.Stop();
                logger.LogInformation("Playwright resources disposal completed in {Elapsed}ms for URL: {Url}", disposalSw.ElapsedMilliseconds, url);
            }
        }
    }
}
