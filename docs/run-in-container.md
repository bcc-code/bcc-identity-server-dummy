# Run the published container image

You can run **BCC Identity Mock** as a container instead of starting the project directly. This is useful when you want a repeatable local setup or want to include the mock in a broader local stack.

## Image

The GitHub Actions workflow publishes the image to:

- `ghcr.io/bcc-code/bcc-identity-server-dummy`

## Pull and run manually

```powershell
docker pull ghcr.io/bcc-code/bcc-identity-server-dummy:latest
docker run --rm -p 12321:8080 ghcr.io/bcc-code/bcc-identity-server-dummy:latest
```

The container listens on port `8080`, so the examples map it to `12321` on the host to align with the default local development URL.

## Configure the image with environment variables

The image uses normal ASP.NET Core configuration binding. Nested configuration keys use double underscores.

Useful keys:

- `ASPNETCORE_HTTP_PORTS`
- `BccPlatform__Environment`
- `BccAuth__ClientId`
- `Client__Bcc__ClientId`
- `Client__Bcc__Secret`
- `Client__Bcc__RedirectUris__0`
- `Client__Bcc__Scopes__0`

## Docker Compose example

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

## Aspire AppHost example

If your local development environment is orchestrated with Aspire, use the published image as a container resource:

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

Use Aspire parameters for environment-specific values and secrets rather than hardcoding them in the AppHost.
