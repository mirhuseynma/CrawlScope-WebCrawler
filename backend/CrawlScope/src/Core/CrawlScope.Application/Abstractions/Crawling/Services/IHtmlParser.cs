using CrawlScope.Application.Abstractions.Crawling.Models;

namespace CrawlScope.Application.Abstractions.Crawling.Services
{
    public interface IHtmlParser
    {
        ParsedPage Parse(string sourceUrl, string html);
    }
}
