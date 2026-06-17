using Microsoft.Extensions.Caching.Memory;

namespace Bcc.Identity.Mock;

public class LogoutContextStore(IMemoryCache cache)
{
    private const string CacheKeyPrefix = "logout_";

    public string Store(string? postLogoutRedirectUri)
    {
        var logoutId = Guid.NewGuid().ToString("N");
        cache.Set(CacheKeyPrefix + logoutId, postLogoutRedirectUri, TimeSpan.FromMinutes(5));
        return logoutId;
    }

    public string? GetPostLogoutRedirectUri(string? logoutId)
    {
        if (string.IsNullOrWhiteSpace(logoutId))
            return null;

        cache.TryGetValue(CacheKeyPrefix + logoutId, out string? postLogoutRedirectUri);
        return postLogoutRedirectUri;
    }
}
