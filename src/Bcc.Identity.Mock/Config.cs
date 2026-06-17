namespace Bcc.Identity.Mock;

using static OpenIddict.Abstractions.OpenIddictConstants;

public static class OpenIddictMockConfiguration
{
    private static readonly HashSet<string> ProtocolScopes =
    [
        Scopes.OpenId,
        Scopes.Profile,
        Scopes.Email,
        Scopes.Address,
        Scopes.Phone,
        Scopes.OfflineAccess
    ];

    public static string[] GetRegisteredScopes(IConfiguration configuration) =>
        GetConfiguredScopes(configuration)
            .Concat(ProtocolScopes)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

    public static IEnumerable<string> GetConfiguredScopes(IConfiguration configuration) =>
        configuration.GetSection("ApiScopes").GetChildren()
            .Select(section => section.Get<ApiScopeSettings>()!.Name)
            .Concat(GetClientScopes(configuration))
            .Distinct(StringComparer.Ordinal);

    public static IEnumerable<string> GetClientScopes(IConfiguration configuration) =>
        configuration.GetSection("Client").GetChildren()
            .SelectMany(section => section.Get<ClientSettings>()?.Scopes ?? []);

    public static bool IsProtocolScope(string scope) => ProtocolScopes.Contains(scope);
}

public class ApiScopeSettings
{
    public required string Name { get; set; }
    public string[] Claims { get; set; } = ["email", "openid", "profile"];
}

public class ClientSettings
{
    public required string Secret { get; set; }
    public required string ClientId { get; set; }
    public string[] RedirectUris { get; set; } = [];
    public string[] Scopes { get; set; } = [];
    public string[] GrantTypes { get; set; } = ["authorization_code", "client_credentials"];
}
