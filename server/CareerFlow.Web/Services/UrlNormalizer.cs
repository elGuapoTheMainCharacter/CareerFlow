namespace CareerFlow.Web.Services;

public static class UrlNormalizer
{
    // Blank input gives null. Valid input gives a clean absolute http(s) URL.
    // Invalid input throws ArgumentException with a readable message.
    public static string? Normalize(string? input, int maxLength = 200)
    {
        if (string.IsNullOrWhiteSpace(input)) return null;

        var text = input.Trim();

        // The user did not type a scheme: assume https.
        if (!text.Contains("://", StringComparison.Ordinal))
            text = "https://" + text;

        const string message = "Please enter a valid website, for example investek.co.za";

        if (!Uri.TryCreate(text, UriKind.Absolute, out var uri)) throw new ArgumentException(message);

        // Only web links. This blocks things like javascript: or ftp:// in a clickable link.
        if (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp)
            throw new ArgumentException(message);

        // A real website has a dot in the host (rejects "hello" and "localhost").
        if (!uri.Host.Contains('.')) throw new ArgumentException(message);

        var result = uri.AbsoluteUri;

        // Uri adds a trailing slash to bare domains. Drop it for a tidier value.
        if (uri.AbsolutePath == "/" && uri.Query.Length == 0 && uri.Fragment.Length == 0)
            result = result.TrimEnd('/');

        if (result.Length > maxLength) throw new ArgumentException("That website address is too long.");

        return result;
    }
}