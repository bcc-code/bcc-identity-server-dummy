using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;

namespace Bcc.Identity.Mock;

public class JustAddAllClaimsProfileService : IProfileService
{
    public Task GetProfileDataAsync(ProfileDataRequestContext context)
    {
        context.IssuedClaims.AddRange(context.Subject.Claims);
        return Task.CompletedTask;
    }

    public Task IsActiveAsync(IsActiveContext context)
    {
        context.IsActive = true;
        return Task.CompletedTask;
    }
}