using Bcc.Members.Identity.Domain.Quickstart.Users;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using static IdentityServer4.IdentityServerConstants;

namespace Bcc.Members.Identity.Domain
{
    public class ProfileService : IProfileService
    {
        protected UserManager<AppUser> _userManager;

        public ProfileService(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {            
            var sub = context.Subject.GetSubjectId();
            var user = await _userManager.GetUserAsync(context.Subject);
            var requestedClaims = context.RequestedResources.ParsedScopes;
            var claims = new List<Claim>();
            foreach (var claim in requestedClaims)
            {
                if(claim.ParsedName == "profile")
                {
                    claims.AddRange(new List<Claim>
                    {
                        new Claim("given_name", user.FirstName, System.Security.Claims.ClaimValueTypes.String),
                        new Claim("family_name", user.LastName, System.Security.Claims.ClaimValueTypes.String),
                        new Claim("nickname", user.Name, System.Security.Claims.ClaimValueTypes.String),
                        new Claim("name", user.Name, System.Security.Claims.ClaimValueTypes.String),                       
                    });
                }

                if (claim.ParsedName == "email")
                {
                    claims.AddRange(new List<Claim>
                    {                        
                        new Claim("email", user.Email, System.Security.Claims.ClaimValueTypes.String),
                    });
                }
            }
            
         
            claims.AddRange(new List<Claim>
            {
                new Claim("https://login.bcc.no/claims/personId", user.personId, System.Security.Claims.ClaimValueTypes.Integer32),
                new Claim("https://login.bcc.no/claims/hasMembership", user.hasMembership.ToString(), System.Security.Claims.ClaimValueTypes.Boolean),
                new Claim("https://login.bcc.no/claims/churchId", user.churchId.ToString(), System.Security.Claims.ClaimValueTypes.Integer32),
                new Claim("https://login.bcc.no/claims/churchName", user.churchName, System.Security.Claims.ClaimValueTypes.String),
            });

            context.IssuedClaims.AddRange(claims);
        }

        public async Task IsActiveAsync(IsActiveContext context)
        {
            //>Processing
            var user = await _userManager.GetUserAsync(context.Subject);

            context.IsActive = true;
        }
    }
}
