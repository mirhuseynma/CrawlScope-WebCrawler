using CrawlScope.Application.Abstractions.Crawling.Services;
using CrawlScope.Application.Modules.Crawling.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace CrawlScope.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddAutoMapper(cfg => cfg.AddMaps(typeof(DependencyInjection).Assembly));
            services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
            services.AddScoped<ICrawlQueueProcessor, CrawlQueueProcessor>();
            return services;
        }
    }
}
