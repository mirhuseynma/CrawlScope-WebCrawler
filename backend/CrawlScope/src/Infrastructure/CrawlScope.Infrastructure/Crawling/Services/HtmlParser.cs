using CrawlScope.Application.Abstractions.Crawling.Models;
using HtmlAgilityPack;
using System.Text.RegularExpressions;

namespace CrawlScope.Infrastructure.Crawling.Services
{
    public class HtmlParser : IHtmlParser
    {
        private const int MaxContentSnapshotLength = 5000;

        public ParsedPage Parse(string sourceUrl, string html)
        {
            var document = new HtmlDocument();
            document.LoadHtml(html);

            var titleNode = document.DocumentNode.SelectSingleNode("//title");
            var title = NormalizeWhitespace(
                titleNode is null ? null : HtmlEntity.DeEntitize(titleNode.InnerText));

            var links = ExtractLinks(sourceUrl, document);
            var textContent = ExtractBodyText(document);

            return new ParsedPage(sourceUrl, title, textContent, links);
        }

        private static string? ExtractBodyText(HtmlDocument document)
        {
            var body = document.DocumentNode.SelectSingleNode("//body");
            if (body is null)
            {
                return null;
            }

            body.SelectNodes(".//script|.//style|.//noscript|.//svg|.//canvas|.//iframe|.//nav|.//header|.//footer|.//aside|.//form|.//button|.//input|.//select|.//textarea|.//option")
                ?.ToList()
                .ForEach(node => node.Remove());

            var textBlocks = body
                .Descendants()
                .Where(IsReadableTextContainer)
                .Select(node => NormalizeWhitespace(HtmlEntity.DeEntitize(node.InnerText)))
                .Where(text => !string.IsNullOrWhiteSpace(text))
                .Select(text => text!)
                .Concat(ExtractLeafTextNodes(body))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var snapshot = textBlocks.Count > 0
                ? string.Join(Environment.NewLine, textBlocks)
                : NormalizeWhitespace(HtmlEntity.DeEntitize(body.InnerText));

            return LimitSnapshot(snapshot);
        }

        private static bool IsReadableTextContainer(HtmlNode node)
        {
            if (node.NodeType != HtmlNodeType.Element || HasReadableChildContainer(node))
            {
                return false;
            }

            return node.Name.Equals("p", StringComparison.OrdinalIgnoreCase)
                || node.Name.Equals("li", StringComparison.OrdinalIgnoreCase)
                || node.Name.Equals("blockquote", StringComparison.OrdinalIgnoreCase)
                || node.Name.Equals("td", StringComparison.OrdinalIgnoreCase)
                || node.Name.Equals("th", StringComparison.OrdinalIgnoreCase)
                || node.Name.Equals("h1", StringComparison.OrdinalIgnoreCase)
                || node.Name.Equals("h2", StringComparison.OrdinalIgnoreCase)
                || node.Name.Equals("h3", StringComparison.OrdinalIgnoreCase)
                || node.Name.Equals("h4", StringComparison.OrdinalIgnoreCase)
                || node.Name.Equals("h5", StringComparison.OrdinalIgnoreCase)
                || node.Name.Equals("h6", StringComparison.OrdinalIgnoreCase);
        }

        private static bool HasReadableChildContainer(HtmlNode node)
        {
            return node.Elements("p").Any()
                || node.Elements("li").Any()
                || node.Elements("blockquote").Any()
                || node.Elements("td").Any()
                || node.Elements("th").Any()
                || node.Elements("h1").Any()
                || node.Elements("h2").Any()
                || node.Elements("h3").Any()
                || node.Elements("h4").Any()
                || node.Elements("h5").Any()
                || node.Elements("h6").Any();
        }

        private static IEnumerable<string> ExtractLeafTextNodes(HtmlNode root)
        {
            return root
                .Descendants()
                .Where(node => node.NodeType == HtmlNodeType.Text)
                .Where(node => !HasReadableAncestor(node))
                .Select(node => NormalizeWhitespace(HtmlEntity.DeEntitize(node.InnerText)))
                .Where(text => !string.IsNullOrWhiteSpace(text))
                .Select(text => text!);
        }

        private static bool HasReadableAncestor(HtmlNode node)
        {
            return node
                .Ancestors()
                .Any(ancestor =>
                    ancestor.Name.Equals("p", StringComparison.OrdinalIgnoreCase)
                    || ancestor.Name.Equals("li", StringComparison.OrdinalIgnoreCase)
                    || ancestor.Name.Equals("blockquote", StringComparison.OrdinalIgnoreCase)
                    || ancestor.Name.Equals("td", StringComparison.OrdinalIgnoreCase)
                    || ancestor.Name.Equals("th", StringComparison.OrdinalIgnoreCase)
                    || ancestor.Name.Equals("h1", StringComparison.OrdinalIgnoreCase)
                    || ancestor.Name.Equals("h2", StringComparison.OrdinalIgnoreCase)
                    || ancestor.Name.Equals("h3", StringComparison.OrdinalIgnoreCase)
                    || ancestor.Name.Equals("h4", StringComparison.OrdinalIgnoreCase)
                    || ancestor.Name.Equals("h5", StringComparison.OrdinalIgnoreCase)
                    || ancestor.Name.Equals("h6", StringComparison.OrdinalIgnoreCase));
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

        private static string? LimitSnapshot(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return value.Length <= MaxContentSnapshotLength
                ? value
                : value[..MaxContentSnapshotLength].TrimEnd();
        }
    }
}
