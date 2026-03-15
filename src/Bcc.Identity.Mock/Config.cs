using Duende.IdentityModel;
using Duende.IdentityServer.Models;

namespace Bcc.Identity.Mock;

public static class Config
{
    public static IEnumerable<IdentityResource> IdentityResources(IConfiguration configuration)
    {
        yield return  new IdentityResources.OpenId();
        yield return  new IdentityResources.Profile();
        yield return  new IdentityResources.Email();

        var identityResources = configuration.GetSection("IdentityResources").GetChildren();
        foreach (var section in identityResources)
        {
            var identityResourceSettings = section.Get<IdentityResourceSettings>()!;
            yield return new IdentityResource(identityResourceSettings.Name, identityResourceSettings.Claims);
        }
    }

    public static IEnumerable<ApiScope> ApiScopes(IConfiguration configuration)
    {
        var apiScopes = configuration.GetSection("ApiScopes").GetChildren();

        foreach (var section in apiScopes)
        {
            var apiScopeSettings = section.Get<ApiScopeSettings>()!;
            yield return new ApiScope(apiScopeSettings.Name, apiScopeSettings.Claims);
        }
    }

    public static IEnumerable<Client> Clients(IConfiguration configuration)
    {
        var clients = configuration.GetSection("Client").GetChildren();

        foreach (var section in clients)
        {
            var clientSettings = section.Get<ClientSettings>()!;
            yield return new Client
            {
                ClientId = clientSettings.ClientId,
                ClientSecrets =
                [
                    new Secret(clientSettings.Secret.Sha256())
                ],
                RedirectUris = clientSettings.RedirectUris,
                PostLogoutRedirectUris = clientSettings.RedirectUris,
                AllowOfflineAccess = true,
                AllowedScopes = clientSettings.Scopes,
                AllowedGrantTypes = clientSettings.GrantTypes,
                AlwaysIncludeUserClaimsInIdToken = true
            };
        }
    }
}

public class IdentityResourceSettings
{
    public required string Name { get; set; }
    public string[] Claims { get; set; } = [];
}

public class ApiScopeSettings
{
    public required string Name { get; set; }

    public string[] Claims { get; set; } =
        [OidcConstants.StandardScopes.Email, OidcConstants.StandardScopes.OpenId, OidcConstants.StandardScopes.Profile];
}

public class ClientSettings
{
    public required string Secret { get; set; }
    public required string ClientId { get; set; }
    public string[] RedirectUris { get; set; } = [];
    public string[] Scopes { get; set; } = [];
    public string[] GrantTypes { get; set; } = [..Duende.IdentityServer.Models.GrantTypes.CodeAndClientCredentials];
}