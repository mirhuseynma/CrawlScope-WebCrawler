using CrawlScope.Application.Abstractions.Export.Services;
using CrawlScope.Infrastructure.Crawling.Services;
using CrawlScope.Infrastructure.Export.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;

namespace CrawlScope.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            services.AddHttpClient<StandardPageFetcher>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(20);
                client.DefaultRequestHeaders.UserAgent.Add(
                    new ProductInfoHeaderValue("CrawlScope", "1.0"));
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("text/html"));
            });

            services.AddTransient<PlaywrightPageFetcher>();

            services.AddHttpClient<CrawlScope.Application.Abstractions.Crawling.Services.IUrlAnalyzerService, UrlAnalyzerService>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(10);
            });

            services.AddScoped<CrawlScope.Application.Abstractions.Crawling.Services.IPageFetcherFactory, PageFetcherFactory>();

            services.AddScoped<IHtmlParser, HtmlParser>();
            services.AddScoped<IExportFileStorage, LocalExportFileStorage>();
            
            services.AddScoped<IExportStrategy, CrawlScope.Infrastructure.Export.Services.CsvExportStrategy>();
            services.AddScoped<IExportStrategy, CrawlScope.Infrastructure.Export.Services.JsonExportStrategy>();
            
            services.AddSingleton<ICrawlJobChannel, CrawlScope.Infrastructure.BackgroundJobs.CrawlJobChannel>();
            services.AddHostedService<CrawlScope.Infrastructure.BackgroundJobs.CrawlJobBackgroundService>();
            services.AddHostedService<CrawlScope.Infrastructure.BackgroundJobs.CrawlJobRecoveryHostedService>();
            
            services.AddHostedService<CrawlScheduleWorker>();

            services.Configure<CrawlScope.Infrastructure.Email.SmtpSettings>(configuration.GetSection("SmtpSettings"));
            services.AddTransient<CrawlScope.Application.Abstractions.Email.IEmailService, CrawlScope.Infrastructure.Email.SmtpEmailService>();

            return services;
        }
    }
}
