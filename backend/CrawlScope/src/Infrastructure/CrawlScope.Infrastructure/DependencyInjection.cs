using CrawlScope.Application.Abstractions.Crawling.Services;
using CrawlScope.Application.Abstractions.Export.Services;
using CrawlScope.Infrastructure.Crawling.Services;
using CrawlScope.Infrastructure.Export.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;

namespace CrawlScope.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            services.AddHttpClient<IPageFetcher, PageFetcher>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(20);
                client.DefaultRequestHeaders.UserAgent.Add(
                    new ProductInfoHeaderValue("CrawlScope", "1.0"));
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("text/html"));
            });

            services.AddScoped<IHtmlParser, HtmlParser>();
            services.AddScoped<IExportFileStorage, LocalExportFileStorage>();

            return services;
        }
    }
}
