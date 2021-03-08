// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4;
using IdentityServer4.Models;
using System.Collections.Generic;
using System.Linq;

namespace Bcc.Members.Identity.Domain
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> Ids =>
            new IdentityResource[]
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResources.Email(),
                new IdentityResource("church",new List<string>(){"churchId"})
            };


        public static IEnumerable<ApiResource> Apis =>
            new ApiResource[]
            {
                new ApiResource("api1", "My API #1")
            };
     
        public static IEnumerable<Client> Clients =>
            new Client[]
            {
               
                // machine to machine client (from quickstart 1)
                new Client
                {
                    ClientId = "client",
                    ClientSecrets = { new Secret("secret".Sha256()) },

                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    // scopes that client has access to
                    AllowedScopes = { "api1" }
                },
                // interactive ASP.NET Core MVC client
                new Client
                {
                    ClientId = "mvc",
                    ClientSecrets = { new Secret("secret".Sha256()) },

                    AllowedGrantTypes = GrantTypes.Code,
                    RequireConsent = false,
                    RequirePkce = true,

                    // where to redirect to after login
                    RedirectUris = { "https://localhost:5002/signin-oidc" },

                    // where to redirect to after logout
                    PostLogoutRedirectUris = { "http://localhost:5002/signout-callback-oidc" },

                    AllowedScopes = new List<string>
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile
                    }
                },
                new Client
                {
                    ClientId = "Auth0",
                    ClientSecrets = { new Secret("mvJu3x$ok%q*f@$0uUCB".Sha256()) },

                    AllowedGrantTypes = GrantTypes.Code,
                    RequireConsent = false,
                    RequirePkce = false,

                    
                    // where to redirect to after login
                    RedirectUris = { "https://devlogin.bcc.no/login/callback","https://login.bcc.no/login/callback", "https://bcc-sso.eu.auth0.com/login/callback", "https://bcc-sso-dev.eu.auth0.com/login/callback" },

                    // where to redirect to after logout
                    PostLogoutRedirectUris = { "http://localhost:5002/signout-callback-oidc","https://bcc-telegram-login.azurewebsites.net/signout-callback-oidc" },

                    AllowedScopes = new List<string>
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email
                    }
                },
                new Client
                {
                    ClientId = "RandomUser",
                    ClientSecrets = { new Secret("RandomUser".Sha256()) },

                    AllowedGrantTypes = 
                    {
                        GrantType.AuthorizationCode,                        
                        GrantType.ClientCredentials,                                                
                        GrantType.ResourceOwnerPassword                        
                    },                    
                    RequireConsent = false,
                    RequirePkce = false,
                    AlwaysIncludeUserClaimsInIdToken = true,
                    RequireClientSecret = false,
                    AllowAccessTokensViaBrowser = true,               
                    

                    RedirectUris = { "https://devlogin.bcc.no/login/callback","https://login.bcc.no/login/callback", "https://bcc-sso.eu.auth0.com/login/callback", "https://bcc-sso-dev.eu.auth0.com/login/callback" },

                    PostLogoutRedirectUris = { "http://localhost:5002/signout-callback-oidc","https://bcc-telegram-login.azurewebsites.net/signout-callback-oidc" },

                    AllowedScopes = new List<string>
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        "church"
                    }                    
                },
                new Client
                {
                    ClientId = "SameUser",
                    ClientSecrets = { new Secret("RandomUser".Sha256()) },

                    AllowedGrantTypes =
                    {
                        GrantType.AuthorizationCode,
                        GrantType.ClientCredentials,
                        GrantType.ResourceOwnerPassword
                    },
                    RequireConsent = false,
                    RequirePkce = false,
                    AlwaysIncludeUserClaimsInIdToken = true,
                    RequireClientSecret = false,
                    AllowAccessTokensViaBrowser = true,
                                     
                   
                    RedirectUris = { "https://devlogin.bcc.no/login/callback","https://login.bcc.no/login/callback", "https://bcc-sso.eu.auth0.com/login/callback", "https://bcc-sso-dev.eu.auth0.com/login/callback" },

                    PostLogoutRedirectUris = { "http://localhost:5002/signout-callback-oidc","https://bcc-telegram-login.azurewebsites.net/signout-callback-oidc" },

                    AllowedScopes = new List<string>
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        "church"
                    }
                }
            };
    }
}