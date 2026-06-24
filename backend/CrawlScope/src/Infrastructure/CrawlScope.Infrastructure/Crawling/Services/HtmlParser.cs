using CrawlScope.Application.Abstractions.Crawling.Models;
using CrawlScope.Application.Abstractions.Crawling.Services;
using HtmlAgilityPack;
using System.Text.RegularExpressions;

namespace CrawlScope.Infrastructure.Crawling.Services
{
    public class HtmlParser : IHtmlParser
    {
        public ParsedPage Parse(string sourceUrl, string html)
        {
            var document = new HtmlDocument();
            document.LoadHtml(html);

            var titleNode = document.DocumentNode.SelectSingleNode("//title");
            var title = NormalizeWhitespace(
                titleNode is null ? null : HtmlEntity.DeEntitize(titleNode.InnerText));

            var textContent = ExtractBodyText(document);
            var links = ExtractLinks(sourceUrl, document);

            return new ParsedPage(sourceUrl, title, textContent, links);
        }

        private static string? ExtractBodyText(HtmlDocument document)
        {
            var body = document.DocumentNode.SelectSingleNode("//body");
            if (body is null)
            {
                return null;
            }

            body.SelectNodes(".//script|.//style|.//noscript")
                ?.ToList()
                .ForEach(node => node.Remove());

            return NormalizeWhitespace(HtmlEntity.DeEntitize(body.InnerText));
        }

        private static IReadOnlyCollection<ParsedLink> ExtractLinks(string sourceUrl, HtmlDocument document)
        {
            if (!Uri.TryCreate(sourceUrl, UriKind.Absolute, out var sourceUri))
            {
                return [];
            }

            var links = new List<ParsedLink>();
            var anchorNodes = document.DocumentNode.SelectNodes("//a[@href]");

            if (anchorNodes is null)
            {
                return links;
            }

            foreach (var anchor in anchorNodes)
            {
                var href = anchor.GetAttributeValue("href", string.Empty).Trim();
                if (ShouldSkipHref(href) || !Uri.TryCreate(sourceUri, href, out var targetUri))
                {
                    continue;
                }

                var normalizedTargetUrl = NormalizeUrl(targetUri);
                var anchorText = NormalizeWhitespace(HtmlEntity.DeEntitize(anchor.InnerText));
                var isExternal = !string.Equals(sourceUri.Host, targetUri.Host, StringComparison.OrdinalIgnoreCase);

                links.Add(new ParsedLink(sourceUrl, normalizedTargetUrl, anchorText, isExternal));
            }

            return links
                .GroupBy(link => link.TargetUrl, StringComparer.OrdinalIgnoreCase)
                .Select(group => group.First())
                .ToList();
        }

        private static bool ShouldSkipHref(string href)
        {
            return string.IsNullOrWhiteSpace(href)
                || href.StartsWith("#", StringComparison.Ordinal)
                || href.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase)
                || href.StartsWith("tel:", StringComparison.OrdinalIgnoreCase)
                || href.StartsWith("javascript:", StringComparison.OrdinalIgnoreCase);
        }

        private static string NormalizeUrl(Uri uri)
        {
            var builder = new UriBuilder(uri)
            {
                Fragment = string.Empty
            };

            return builder.Uri.ToString();
        }

        private static string? NormalizeWhitespace(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return Regex.Replace(value, "\\s+", " ").Trim();
        }
    }
}
