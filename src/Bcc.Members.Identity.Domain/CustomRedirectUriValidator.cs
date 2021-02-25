using IdentityServer4.Models;
using IdentityServer4.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bcc.Members.Identity.Domain
{
    public class CustomRedirectUriValidator : IRedirectUriValidator
    {
        public async Task<bool> IsPostLogoutRedirectUriValidAsync(string requestedUri, Client client)
        {
            
            return true;
        }

        public async Task<bool> IsRedirectUriValidAsync(string requestedUri, Client client)
        {
            
            return true;
        }
    }
}
