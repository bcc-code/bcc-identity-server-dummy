using Bcc.Members.Identity.Domain.Quickstart.Users;
using IdentityModel;
using IdentityServer4;
using IdentityServer4.Configuration;
using IdentityServer4.EntityFramework.Entities;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using static IdentityServer4.Events.TokenIssuedSuccessEvent;

namespace Bcc.Members.Identity.Domain.Controllers
{
    //Ref:
    // https://github.com/IdentityServer/IdentityServer4/blob/main/src/IdentityServer4/src/Services/Default/DefaultTokenService.cs


    [Route("api/[controller]")]
    [ApiController]
    public class UtilitiesController : ControllerBase
    {
        private readonly ITokenCreationService _tokenService;
        private readonly ISystemClock _clock;
        private readonly UserManager<AppUser> _userManager;

        public UtilitiesController(ITokenCreationService tokenService, ISystemClock clock, UserManager<AppUser> userManager)
        {
            _tokenService = tokenService;
            _clock = clock;
            _userManager = userManager;
        }

        [Route("generate-id-tokens")]
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<List<string>>> GenerateIdTokensAsync(string clientId, int count = 100)
        {
            clientId = clientId ?? Config.Clients.FirstOrDefault().ClientId;
            var tokens = new List<string>();
            var users = _userManager.Users.Take(count);

            foreach (var user in users)
            {

                var claims = new List<Claim>()
                {
                    new Claim("sub", user.Id),
                    new Claim("email", user.Email)
                };

                var oToken = new IdentityServer4.Models.Token(OidcConstants.TokenTypes.IdentityToken)
                {
                    CreationTime = _clock.UtcNow.UtcDateTime,
                    Audiences = { clientId },
                    Issuer = $"{Request.Scheme}://{Request.Host}",
                    Lifetime = 10000,
                    Claims = claims.Distinct(new ClaimComparer()).ToList(),
                    ClientId = clientId,
                    AccessTokenType = AccessTokenType.Jwt,
                    AllowedSigningAlgorithms = IdentityServerConstants.SupportedSigningAlgorithms.ToArray(),
                };

                var sToken = await _tokenService.CreateTokenAsync(oToken);
                tokens.Add(sToken);
            }


            return Ok(tokens);
        }


        [Route("generate-access-tokens")]
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<List<string>>> GenerateAccessTokensAsync(string clientId, string audience, int count = 100)
        {
            clientId = clientId ?? Config.Clients.FirstOrDefault().ClientId;
            var tokens = new List<string>();
            var users = _userManager.Users.Take(count);

            foreach (var user in users)
            {
                var claims = new List<Claim>()
                {
                    new Claim("sub", user.Id),
                    new Claim(JwtClaimTypes.IssuedAt, _clock.UtcNow.ToUnixTimeSeconds().ToString(),ClaimValueTypes.Integer64)
                };

                var oToken = new IdentityServer4.Models.Token(OidcConstants.TokenTypes.AccessToken)
                {
                    CreationTime = _clock.UtcNow.UtcDateTime,
                    Audiences = { audience ?? "bcc.members" },
                    Issuer = $"{Request.Scheme}://{Request.Host}",
                    Lifetime = 10000,
                    Claims = claims.Distinct(new ClaimComparer()).ToList(),
                    ClientId = clientId,
                    AccessTokenType = AccessTokenType.Jwt,
                    AllowedSigningAlgorithms = IdentityServerConstants.SupportedSigningAlgorithms.ToArray(),
                };

                var sToken = await _tokenService.CreateTokenAsync(oToken);
                tokens.Add(sToken);
            }


            return Ok(tokens);
        }
    }
}
