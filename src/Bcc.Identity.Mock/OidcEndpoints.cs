using System.Security.Claims;
using Duende.IdentityModel;
using Duende.IdentityServer;
using Duende.IdentityServer.Extensions;
using Duende.IdentityServer.Services;
using Microsoft.AspNetCore.Authentication;

namespace Bcc.Identity.Mock;

public static class OidcEndpoints
{
    public static void MapOidcEndpoints(this WebApplication app)
    {
        var oidc = app.MapGroup("spa");

        oidc.MapGet("context", Context);
        oidc.MapPost("login", Login);
        oidc.MapGet("error", Error);
        oidc.MapGet("logout", Logout);
        oidc.MapPost("logout", PostLogout);
        oidc.MapGet("diagnostics", async (HttpContext context) =>
        {
            var diagnostics = await Diagnostics(context);
            return diagnostics;
        });
    }

    public static async Task<IResult> Context(string returnUrl, IIdentityServerInteractionService interaction)
    {
        var authzContext = await interaction.GetAuthorizationContextAsync(returnUrl);
        if (authzContext != null)
        {
            return Results.Ok(new
            {
                loginHint = authzContext.LoginHint,
                idp = authzContext.IdP,
                tenant = authzContext.Tenant,
                scopes = authzContext.ValidatedResources.RawScopeValues,
                client = authzContext.Client.ClientName ?? authzContext.Client.ClientId
            });
        }

        return Results.BadRequest();
    }

    public static async Task<IResult> Login(LoginRequest model, IIdentityServerInteractionService interaction,
        IServerUrls serverUrls, HttpContext context)
    {
        var response = new LoginResponse();

        var url = model.ReturnUrl != null ? Uri.UnescapeDataString(model.ReturnUrl) : null;

        var authzContext = await interaction.GetAuthorizationContextAsync(url);
        response.ValidReturnUrl = authzContext != null ? url : serverUrls.BaseUrl;

        var user = new IdentityServerUser(model.Email)
        {
            DisplayName = model.Email,
            AdditionalClaims = new List<Claim>()
            {
                new(JwtClaimTypes.Name, model.Email),
                new(JwtClaimTypes.Email, model.Email),
            }
        };

        AuthenticationProperties props = new AuthenticationProperties()
        {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(5),
        };
        await context.SignInAsync(user, props);

        return Results.Ok(response);
    }

    public static async Task<IResult> Error(string errorId, IIdentityServerInteractionService interaction)
    {
        var error = await interaction.GetErrorContextAsync(errorId);
        if (error != null)
        {
            return Results.Ok(new
            {
                error.Error,
                error.ErrorDescription
            });
        }

        return Results.BadRequest();
    }

    public static async Task<IResult> PostLogout(string logoutId, IIdentityServerInteractionService interaction, HttpContext context)
    {
        var logoutInfo = await interaction.GetLogoutContextAsync(logoutId);

        await context.SignOutAsync();

        return Results.Ok(new
        {
            postLogoutRedirectUri = logoutInfo?.PostLogoutRedirectUri
        });
    }


    private static async Task<IResult> Logout(string? logoutId, IIdentityServerInteractionService interaction, HttpContext context)
    {
        try
        {
            var logoutInfo = await interaction.GetLogoutContextAsync(logoutId);

            if (context.User.IsAuthenticated())
            {
                await context.SignOutAsync();

                return Results.Ok(new
                {
                    postLogoutRedirectUri = logoutInfo.PostLogoutRedirectUri ?? "https://localhost:15000",
                });
            }
        }
        catch (Exception e)
        {
            // ignored
        }

        return Results.Ok(new
        {
            prompt = context.User.IsAuthenticated()
        });
    }

    private static async Task<IResult> Diagnostics(HttpContext context)
    {
        return Results.Ok(new DiagnosticsDto(await context.AuthenticateAsync()));
    }
}

public class LoginRequest
{
    public string Email { get; set; }
    public string? ReturnUrl { get; set; }
}

public class LoginResponse
{
    public string? ValidReturnUrl { get; set; }
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