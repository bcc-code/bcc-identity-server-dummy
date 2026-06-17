# BCC Identity Mock

This repository contains a small ASP.NET Core OpenIddict-based mock identity server used for local development and integration testing.

## What it provides

- OIDC endpoints at `/connect/authorize`, `/connect/token`, `/connect/userinfo`, and `/connect/endsession`
- A simple login UI at `/login`
- A diagnostics endpoint at `/spa/diagnostics`
- Seeded client and scope configuration from `src/Bcc.Identity.Mock/appsettings.json`

## Run locally

```powershell
dotnet run --project .\src\Bcc.Identity.Mock\Bcc.Identity.Mock.csproj
```

The local development profile listens on `https://localhost:12321`.

## Container publishing

The workflow in `.github/workflows/publish-container.yml` publishes the app to GitHub Container Registry with `dotnet publish` and the .NET SDK container target, so no Dockerfile is required. The publish step targets Linux x64 and enables ReadyToRun compilation for the produced image.

- Pushes to `main` publish `ghcr.io/bcc-code/bcc-identity-server-dummy` with the tags `latest`, `main`, and `sha-<commit>`
- Tags that start with `v` also publish versioned tags and refresh `latest`
- `workflow_dispatch` can be used to publish manually

Example:

```powershell
docker pull ghcr.io/bcc-code/bcc-identity-server-dummy:latest
docker run --rm -p 8080:8080 ghcr.io/bcc-code/bcc-identity-server-dummy:latest
```

## Configure the container image

The image uses normal ASP.NET Core configuration binding, so settings from `src/Bcc.Identity.Mock/appsettings.json` can be overridden with environment variables.

Common keys:

- `BccPlatform__Environment`
- `BccAuth__ClientId`
- `Client__Bcc__ClientId`
- `Client__Bcc__Secret`
- `Client__Bcc__RedirectUris__0`
- `Client__Bcc__Scopes__0`
- `ApiScopes__person#read__Name`

### Docker Compose

```yaml
services:
  identity-mock:
    image: ghcr.io/bcc-code/bcc-identity-server-dummy:latest
    ports:
      - "12321:8080"
    environment:
      ASPNETCORE_HTTP_PORTS: "8080"
      BccPlatform__Environment: "Sandbox"
      BccAuth__ClientId: "your-client-id"
      Client__Bcc__ClientId: "bcc-client"
      Client__Bcc__Secret: "bcc-secret"
      Client__Bcc__RedirectUris__0: "http://host.docker.internal:3301/signin-oidc"
      Client__Bcc__Scopes__0: "openid"
      Client__Bcc__Scopes__1: "profile"
      Client__Bcc__Scopes__2: "email"
      Client__Bcc__Scopes__3: "personUid"
      Client__Bcc__Scopes__4: "person#read"
      Client__Bcc__Scopes__5: "offline_access"
```

Notes:

- `__` maps nested configuration sections in ASP.NET Core
- arrays use numeric indexes such as `RedirectUris__0` and `Scopes__0`
- map host port `12321` to container port `8080` if you want the container to match the existing local dev URL shape more closely

### Aspire

In an Aspire AppHost, add the published image as a container resource and pass the same environment variables with `WithEnvironment(...)`:

```csharp
var builder = DistributedApplication.CreateBuilder(args);

var bccAuthClientId = builder.AddParameter("bcc-auth-client-id");

builder.AddContainer("identity-mock", "ghcr.io/bcc-code/bcc-identity-server-dummy", "latest")
    .WithHttpEndpoint(port: 12321, targetPort: 8080)
    .WithEnvironment("ASPNETCORE_HTTP_PORTS", "8080")
    .WithEnvironment("BccPlatform__Environment", "Sandbox")
    .WithEnvironment("BccAuth__ClientId", bccAuthClientId)
    .WithEnvironment("Client__Bcc__ClientId", "bcc-client")
    .WithEnvironment("Client__Bcc__Secret", "bcc-secret")
    .WithEnvironment("Client__Bcc__RedirectUris__0", "http://localhost:3301/signin-oidc")
    .WithEnvironment("Client__Bcc__Scopes__0", "openid")
    .WithEnvironment("Client__Bcc__Scopes__1", "profile")
    .WithEnvironment("Client__Bcc__Scopes__2", "email")
    .WithEnvironment("Client__Bcc__Scopes__3", "personUid")
    .WithEnvironment("Client__Bcc__Scopes__4", "person#read")
    .WithEnvironment("Client__Bcc__Scopes__5", "offline_access");
```

Use Aspire parameters for values you don't want hardcoded in the AppHost, especially secrets or environment-specific IDs.
