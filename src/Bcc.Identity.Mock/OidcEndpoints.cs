using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Bcc.Identity.Mock;

public static class OidcEndpoints
{
    public static void MapOidcEndpoints(this WebApplication app)
    {
        app.MapMethods("/connect/authorize", ["GET", "POST"], (Delegate)AuthorizeAsync);
        app.MapPost("/connect/token", (Delegate)ExchangeAsync);
        app.MapMethods("/connect/userinfo", ["GET", "POST"], (Delegate)UserInfoAsync);
        app.MapMethods("/connect/endsession", ["GET", "POST"], (Delegate)EndSessionAsync);

        app.MapGet("spa/diagnostics", async (HttpContext context) =>
        {
            var result = await context.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Results.Ok(new DiagnosticsDto(result));
        });
    }

    private static async Task<IResult> AuthorizeAsync(HttpContext ctx)
    {
        var request = ctx.GetOpenIddictServerRequest()
            ?? throw new InvalidOperationException("The OpenIddict server request cannot be retrieved.");

        var cookie = await ctx.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        if (!cookie.Succeeded)
        {
            // Not logged in — redirect to login page, preserving the full authorize URL as the return URL
            var returnUrl = ctx.Request.PathBase + ctx.Request.Path
                + QueryString.Create(ctx.Request.Query.ToList());
            return Results.Challenge(
                new AuthenticationProperties { RedirectUri = returnUrl },
                [CookieAuthenticationDefaults.AuthenticationScheme]);
        }

        var identity = new ClaimsIdentity(
            authenticationType: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
            nameType: Claims.Name,
            roleType: Claims.Role);

        var sub = cookie.Principal!.FindFirstValue(Claims.Subject)
                  ?? cookie.Principal.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? throw new InvalidOperationException("No subject claim found in the session.");

        identity.AddClaim(new System.Security.Claims.Claim(Claims.Subject, sub)
            .SetDestinations(Destinations.AccessToken, Destinations.IdentityToken));

        // Forward all other cookie claims into the token (mirrors JustAddAllClaimsProfileService)
        foreach (var claim in cookie.Principal.Claims
            .Where(c => c.Type != Claims.Subject && c.Type != ClaimTypes.NameIdentifier))
        {
            identity.AddClaim(claim.SetDestinations(Destinations.AccessToken, Destinations.IdentityToken));
        }

        var principal = new ClaimsPrincipal(identity);
        ApplyScopes(principal, request.GetScopes());

        return Results.SignIn(principal, null, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    private static async Task<IResult> ExchangeAsync(HttpContext ctx)
    {
        var request = ctx.GetOpenIddictServerRequest();

        if (request is null)
            throw new InvalidOperationException("The OpenIddict server request cannot be retrieved.");

        if (request.IsClientCredentialsGrantType())
        {
            var clientId = request.ClientId
                ?? throw new InvalidOperationException("The client identifier cannot be retrieved.");

            var identity = new ClaimsIdentity(
                authenticationType: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                nameType: Claims.Name,
                roleType: Claims.Role);

            identity.AddClaim(new System.Security.Claims.Claim(Claims.Subject, clientId)
                .SetDestinations(Destinations.AccessToken));
            identity.AddClaim(new System.Security.Claims.Claim(Claims.Name, clientId)
                .SetDestinations(Destinations.AccessToken));
            identity.AddClaim(new System.Security.Claims.Claim(Claims.ClientId, clientId)
                .SetDestinations(Destinations.AccessToken));

            var principal = new ClaimsPrincipal(identity);
            ApplyScopes(principal, request.GetScopes());

            return Results.SignIn(principal, null, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        if (request.IsAuthorizationCodeGrantType() || request.IsRefreshTokenGrantType())
        {
            var result = await ctx.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            if (!result.Succeeded || result.Principal is null)
            {
                return Results.Forbid(
                    CreateErrorProperties(Errors.InvalidGrant, "The token request is no longer valid."),
                    [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme]);
            }

            var identity = new ClaimsIdentity(
                result.Principal.Claims,
                authenticationType: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                nameType: Claims.Name,
                roleType: Claims.Role);

            var principal = new ClaimsPrincipal(identity);
            ApplyScopes(principal, result.Principal.GetScopes());

            return Results.SignIn(principal, null, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        return Results.Forbid(
            CreateErrorProperties(Errors.UnsupportedGrantType, "The specified grant type is not supported."),
            [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme]);
    }

    private static async Task<IResult> UserInfoAsync(HttpContext ctx)
    {
        var result = await ctx.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        if (!result.Succeeded || result.Principal is null)
        {
            return Results.Forbid(
                CreateErrorProperties(Errors.InvalidToken, "The access token is not valid."),
                [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme]);
        }

        var payload = result.Principal.Claims
            .Where(claim => !claim.Type.StartsWith("oi_", StringComparison.Ordinal))
            .GroupBy(claim => claim.Type, StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => group.Count() == 1
                    ? (object)group.First().Value
                    : group.Select(claim => claim.Value).ToArray(),
                StringComparer.Ordinal);

        return Results.Text(JsonSerializer.Serialize(payload), "application/json");
    }

    private static IResult EndSessionAsync(HttpContext ctx, LogoutContextStore logoutContextStore)
    {
        var request = ctx.GetOpenIddictServerRequest();
        var logoutId = logoutContextStore.Store(request?.PostLogoutRedirectUri);
        return Results.Redirect($"/logout?logoutId={Uri.EscapeDataString(logoutId)}");
    }

    private static void ApplyScopes(ClaimsPrincipal principal, IEnumerable<string> scopes)
    {
        var grantedScopes = scopes
            .Where(scope => !string.IsNullOrWhiteSpace(scope))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        principal.SetScopes(grantedScopes);
        principal.SetResources(
            grantedScopes.Where(scope => !OpenIddictMockConfiguration.IsProtocolScope(scope)));
    }

    private static AuthenticationProperties CreateErrorProperties(string error, string description) =>
        new(new Dictionary<string, string?>
        {
            [OpenIddictServerAspNetCoreConstants.Properties.Error] = error,
            [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = description
        });
}

public class DiagnosticsDto
{
    public DiagnosticsDto(AuthenticateResult result)
    {
        Authenticated = result.Succeeded;
        Failure = result.Failure;
        Claims = result.Principal?.Claims.Select(x => x.ToString());
        Properties = result.Properties;
    }

    public AuthenticationProperties? Properties { get; set; }
    public IEnumerable<string>? Claims { get; set; }
    public Exception? Failure { get; set; }
    public bool Authenticated { get; set; }
}
