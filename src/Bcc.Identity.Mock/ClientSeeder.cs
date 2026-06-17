using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Bcc.Identity.Mock;

public class ClientSeeder : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;

    public ClientSeeder(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        await SeedScopesAsync(scope.ServiceProvider, cancellationToken);
        await SeedClientsAsync(scope.ServiceProvider, cancellationToken);
    }

    private async Task SeedScopesAsync(IServiceProvider services, CancellationToken ct)
    {
        var scopeManager = services.GetRequiredService<IOpenIddictScopeManager>();

        foreach (var scopeName in OpenIddictMockConfiguration.GetConfiguredScopes(_configuration)
                     .Where(scope => !OpenIddictMockConfiguration.IsProtocolScope(scope)))
        {
            if (await scopeManager.FindByNameAsync(scopeName, ct) is null)
                await scopeManager.CreateAsync(new OpenIddictScopeDescriptor { Name = scopeName }, ct);
        }
    }

    private async Task SeedClientsAsync(IServiceProvider services, CancellationToken ct)
    {
        var appManager = services.GetRequiredService<IOpenIddictApplicationManager>();

        foreach (var section in _configuration.GetSection("Client").GetChildren())
        {
            var settings = section.Get<ClientSettings>()!;
            if (await appManager.FindByClientIdAsync(settings.ClientId, ct) is not null)
                continue;

            var descriptor = new OpenIddictApplicationDescriptor
            {
                ClientId = settings.ClientId,
                ClientSecret = settings.Secret,
                ClientType = ClientTypes.Confidential,
                DisplayName = settings.ClientId,
            };

            foreach (var uri in settings.RedirectUris)
                descriptor.RedirectUris.Add(new Uri(uri));
            foreach (var uri in settings.RedirectUris)
                descriptor.PostLogoutRedirectUris.Add(new Uri(uri));

            descriptor.Permissions.Add(Permissions.Endpoints.Authorization);
            descriptor.Permissions.Add(Permissions.Endpoints.Token);
            descriptor.Permissions.Add(Permissions.Endpoints.EndSession);
            descriptor.Permissions.Add(Permissions.Endpoints.Revocation);

            foreach (var grantType in settings.GrantTypes)
            {
                descriptor.Permissions.Add(grantType switch
                {
                    "authorization_code" => Permissions.GrantTypes.AuthorizationCode,
                    "client_credentials" => Permissions.GrantTypes.ClientCredentials,
                    "refresh_token"      => Permissions.GrantTypes.RefreshToken,
                    _                    => Permissions.Prefixes.GrantType + grantType
                });
            }

            if (settings.GrantTypes.Contains("authorization_code"))
            {
                descriptor.Permissions.Add(Permissions.GrantTypes.RefreshToken);
                descriptor.Permissions.Add(Permissions.ResponseTypes.Code);
            }

            // Skip offline_access — controlled by RefreshToken grant type, not a scope permission
            foreach (var s in settings.Scopes.Where(s => s != "offline_access"))
            {
                descriptor.Permissions.Add(s switch
                {
                    "profile" => Permissions.Scopes.Profile,
                    "email"   => Permissions.Scopes.Email,
                    "address" => Permissions.Scopes.Address,
                    "phone"   => Permissions.Scopes.Phone,
                    _         => Permissions.Prefixes.Scope + s
                });
            }

            // openid is always required (no dedicated constant in 7.x — use the prefix form)
            descriptor.Permissions.Add(Permissions.Prefixes.Scope + Scopes.OpenId);

            await appManager.CreateAsync(descriptor, ct);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
