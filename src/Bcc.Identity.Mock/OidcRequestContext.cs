using Microsoft.AspNetCore.WebUtilities;

namespace Bcc.Identity.Mock;

public record OidcRequestContext(string? ClientId, string? LoginHint, string[] Scopes)
{
    public static OidcRequestContext Parse(string? returnUrl)
    {
        if (string.IsNullOrWhiteSpace(returnUrl))
            return new OidcRequestContext(null, null, []);

        var uri = BuildUri(returnUrl);
        var query = QueryHelpers.ParseQuery(uri.Query);
        var scopes = Get(query, "scope")
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return new OidcRequestContext(
            Get(query, "client_id"),
            Get(query, "login_hint"),
            scopes);
    }

    public static string GetLocalReturnUrl(string? returnUrl)
    {
        if (string.IsNullOrWhiteSpace(returnUrl))
            return "/";

        var decoded = Uri.UnescapeDataString(returnUrl);
        if (Uri.TryCreate(decoded, UriKind.Absolute, out var absolute))
            decoded = absolute.PathAndQuery;

        return decoded.StartsWith("/connect/authorize", StringComparison.OrdinalIgnoreCase)
            ? decoded
            : "/";
    }

    private static string Get(Dictionary<string, Microsoft.Extensions.Primitives.StringValues> query, string key) =>
        query.TryGetValue(key, out var value) ? value.ToString() : string.Empty;

    private static Uri BuildUri(string returnUrl)
    {
        var decoded = Uri.UnescapeDataString(returnUrl);

        if (Uri.TryCreate(decoded, UriKind.Absolute, out var absolute))
            return absolute;

        var relative = decoded.StartsWith('/') ? decoded : "/" + decoded;
        return new Uri($"https://localhost{relative}", UriKind.Absolute);
    }
}
