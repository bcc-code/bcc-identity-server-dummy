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

The local development profile listens on `https://localhost:15002`.

## Container publishing

The workflow in `.github/workflows/publish-container.yml` publishes the app to GitHub Container Registry with `dotnet publish` and the .NET SDK container target, so no Dockerfile is required.

- Pushes to `main` publish `ghcr.io/bcc-code/bcc-identity-server-dummy` with the tags `latest`, `main`, and `sha-<commit>`
- Tags that start with `v` also publish versioned tags and refresh `latest`
- `workflow_dispatch` can be used to publish manually

Example:

```powershell
docker pull ghcr.io/bcc-code/bcc-identity-server-dummy:latest
docker run --rm -p 8080:8080 ghcr.io/bcc-code/bcc-identity-server-dummy:latest
```

If the application needs extra configuration in a container, pass it with environment variables in the usual ASP.NET Core way.
