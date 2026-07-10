using AutoMapper;
using CrawlScope.Application.Modules.Crawling.Mappings;
using Microsoft.Extensions.Logging.Abstractions;

namespace CrawlScope.Application.Tests.Common;

internal static class TestMapperFactory
{
    public static IMapper Create()
    {
        var configuration = new MapperConfiguration(
            configuration =>
            {
                configuration.AddProfile<CrawlingProfile>();
            },
            NullLoggerFactory.Instance);

        return configuration.CreateMapper();
    }
}
