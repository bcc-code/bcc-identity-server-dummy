// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Collections.Generic;

namespace IdentityServer4.Quickstart.UI
{
    public class SecurityHeadersAttribute : ActionFilterAttribute
    {
        public override void OnResultExecuting(ResultExecutingContext context)
        {
            var result = context.Result;
            if (result is ViewResult)
            {
                // https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/X-Content-Type-Options
                if (!context.HttpContext.Response.Headers.ContainsKey("X-Content-Type-Options"))
                {
                    context.HttpContext.Response.Headers.Add("X-Content-Type-Options", "nosniff");
                }

                // https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/X-Frame-Options
                if (!context.HttpContext.Response.Headers.ContainsKey("X-Frame-Options"))
                {
                    context.HttpContext.Response.Headers.Add("X-Frame-Options", "SAMEORIGIN");
                }

                #region BCC customization
                var allowedOrigins = new List<string>()
                {
                    "https://telegram.org https://oauth.telegram.org",
                    "https://stackpath.bootstrapcdn.com",
                    "https://a3e811289234f4ab3b16db2d.blob.core.windows.net",
                    "https://syncbrunstaddatatauth0.blob.core.windows.net",
                    "https://az416426.vo.msecnd.net/scripts/b/ai.2.min.js",
                    "'unsafe-eval'",
                    "'unsafe-inline'"
                };

                var allowedSandboxPermissions = new List<string>()
                {
                    // in order for the telegram widget make the popup screen
                    "allow-popups"
                };
                #endregion


                // https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Content-Security-Policy
                var csp = $"default-src  'self' {string.Join(" ",allowedOrigins)}; object-src 'none'; sandbox {string.Join(" ", allowedSandboxPermissions)} allow-forms allow-same-origin allow-scripts; base-uri 'self';";
                // also consider adding upgrade-insecure-requests once you have HTTPS in place for production
                //csp += "upgrade-insecure-requests;";
                // also an example if you need client images to be displayed from twitter
                // csp += "img-src 'self' https://pbs.twimg.com;";

                
                // once for standards compliant browsers
                if (!context.HttpContext.Response.Headers.ContainsKey("Content-Security-Policy"))
                {
                    context.HttpContext.Response.Headers.Add("Content-Security-Policy", csp);
                }
                // and once again for IE
                if (!context.HttpContext.Response.Headers.ContainsKey("X-Content-Security-Policy"))
                {
                    context.HttpContext.Response.Headers.Add("X-Content-Security-Policy", csp);
                }

                // https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Referrer-Policy
                var referrer_policy = "no-referrer";
                if (!context.HttpContext.Response.Headers.ContainsKey("Referrer-Policy"))
                {
                    context.HttpContext.Response.Headers.Add("Referrer-Policy", referrer_policy);
                }
            }
        }
    }
}
