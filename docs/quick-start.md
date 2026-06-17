# Quick start for local development

Use **BCC Identity Mock** when your application needs an OpenID Connect provider locally, but you do not want to depend on the Auth0 sandbox login experience during development.

## Prerequisites

- .NET 10 SDK if you want to run the project directly
- access to the dependencies needed by the app to populate the login UI with people
- a local application that can be configured to use a custom OIDC authority

## Start the mock locally

```powershell
dotnet run --project .\src\Bcc.Identity.Mock\Bcc.Identity.Mock.csproj
```

By default, the local development profile listens on:

- `https://localhost:12321`

Useful endpoints:

- discovery document: `https://localhost:12321/.well-known/openid-configuration`
- login page: `https://localhost:12321/login`
- logout page: `https://localhost:12321/logout`
- diagnostics: `https://localhost:12321/spa/diagnostics`

## Typical developer workflow

1. Start the mock identity server.
2. Configure your local application to use `https://localhost:12321` as its OIDC authority.
3. Make sure the application's redirect URI matches one of the configured redirect URIs in the mock.
4. Start your application and trigger sign-in.
5. On the mock login page, choose an organization and a person.
6. Complete the login and continue testing your application locally.

## Default client configuration

The default seeded client in `src/Bcc.Identity.Mock/appsettings.json` is:

- client ID: `bcc-client`
- client secret: `bcc-secret`
- redirect URI: `http://localhost:3301/signin-oidc`
- scopes: `openid profile email personUid person#read offline_access`

If your application uses a different redirect URI, client ID, or secret, override the mock configuration before you start it.

## What happens during login

The mock uses a local login UI instead of Auth0:

1. Your application redirects the user to `/connect/authorize`.
2. If no local cookie exists, the mock redirects to `/login`.
3. The developer chooses a person from the UI.
4. The mock issues tokens locally and redirects back to the application's configured redirect URI.

This makes it easy to switch between users during development without needing repeated Auth0 sandbox logins.

## Limitations to keep in mind

- the signing and encryption keys are ephemeral, so restarting the app invalidates existing tokens
- the OpenIddict store is in memory, so seeded data is recreated on startup
- the behavior is close enough for local integration testing, but not a full sandbox replica
- the available people depend on what the app can load for the login selector

## Troubleshooting

### Redirect URI mismatch

If sign-in fails after redirecting back from the mock, make sure the redirect URI used by your application matches the configured client entry in the mock.

### No people appear on the login page

The login UI depends on the app being able to load people for the selector. Check the app configuration and runtime logs if the organization and person lists are empty.

### Existing tokens stop working after restart

That is expected. Restarting the mock recreates ephemeral keys and in-memory state.
