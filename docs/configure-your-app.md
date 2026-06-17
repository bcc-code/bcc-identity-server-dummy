# Configure your application to use the mock

This guide explains how to point a local application at **BCC Identity Mock** instead of the Auth0 sandbox environment.

## Minimum values to change

Most consuming applications only need these values changed for local development:

- **authority / issuer**: `https://localhost:12321`
- **client ID**: `bcc-client` by default
- **client secret**: `bcc-secret` by default
- **redirect URI**: must match a redirect URI configured in the mock client
- **requested scopes**: match the scopes configured in the mock client

## Default client and scope values

The mock seeds client settings from `src/Bcc.Identity.Mock/appsettings.json`.

Default `Client:Bcc` values:

- `ClientId`: `bcc-client`
- `Secret`: `bcc-secret`
- `RedirectUris[0]`: `http://localhost:3301/signin-oidc`
- `Scopes`: `openid`, `profile`, `email`, `personUid`, `person#read`, `offline_access`

Default API scope:

- `person#read`

## Example: local app configuration

The exact setting names depend on the consuming application, but the values typically look like this:

```json
{
  "Authentication": {
    "Authority": "https://localhost:12321",
    "ClientId": "bcc-client",
    "ClientSecret": "bcc-secret",
    "Scopes": [ "openid", "profile", "email", "personUid", "person#read", "offline_access" ]
  }
}
```

## When you need custom client settings

If your application uses different local settings, update the mock to match.

Common reasons:

- your app runs on another port
- your redirect URI is different
- your app uses a different client ID
- you want a different set of scopes

The mock supports standard ASP.NET Core configuration overrides, so you can change seeded values with environment variables instead of editing `appsettings.json`.

Examples:

- `Client__Bcc__ClientId=my-local-client`
- `Client__Bcc__Secret=my-local-secret`
- `Client__Bcc__RedirectUris__0=http://localhost:5173/signin-oidc`
- `Client__Bcc__Scopes__0=openid`
- `Client__Bcc__Scopes__1=profile`

## Verify that your app is using the mock

Good checks:

1. open the discovery document at `https://localhost:12321/.well-known/openid-configuration`
2. trigger login from your application
3. confirm the browser redirects to `https://localhost:12321/login`
4. after login, inspect the token or user info response from the mock

## Diagnostics endpoint

The mock exposes:

- `https://localhost:12321/spa/diagnostics`

Use it after signing in to inspect the local authentication cookie and claims that the mock currently sees.

## Claims and behavior notes

The mock forwards the selected user's claims into issued tokens. It also issues a `personUid`-related claim used by downstream applications.

Because this is a development-oriented local provider:

- tokens are not meant to be stable across restarts
- production Auth0-specific behavior should not be assumed
- the goal is fast local iteration, not full identity-provider parity
