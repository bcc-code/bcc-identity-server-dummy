# BCC Identity Mock developer guides

These guides are intended for developers who want to use **BCC Identity Mock** during local development instead of signing in through the shared Auth0 sandbox environment.

## Guides

- [Quick start for local development](./quick-start.md)
- [Configure your application to use the mock](./configure-your-app.md)
- [Run the published container image](./run-in-container.md)

## What this mock is for

Use this mock when you need to:

- develop login-dependent flows locally
- test OIDC redirects without going through Auth0 sandbox logins
- switch users quickly during manual testing
- test client credentials or refresh-token flows against a local issuer

## What this mock is not

This project is a local development tool. It is not intended to behave exactly like the production or sandbox Auth0 environments.

Notable differences:

- users are selected from a local login UI instead of logging in with Auth0
- signing keys are ephemeral and recreated on restart
- OpenIddict data is stored in memory
- behavior is optimized for local development and integration testing, not production parity
