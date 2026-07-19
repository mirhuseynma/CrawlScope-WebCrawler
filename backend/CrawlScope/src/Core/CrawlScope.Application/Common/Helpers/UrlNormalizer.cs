namespace CrawlScope.Application.Common.Helpers
{
    public static class UrlNormalizer
    {
        public static string Normalize(string? url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return string.Empty;
            }

            if (Uri.TryCreate(url.Trim(), UriKind.Absolute, out var uri))
            {
                return Normalize(uri);
            }

            return url.Trim().TrimEnd('/');
        }

        public static string Normalize(Uri uri)
        {
            var builder = new UriBuilder(uri)
            {
                Fragment = string.Empty
            };

            var normalized = builder.Uri.ToString();
            if (normalized.EndsWith('/') && !normalized.EndsWith("://"))
            {
                normalized = normalized.TrimEnd('/');
            }

            return normalized;
        }
    }
}
