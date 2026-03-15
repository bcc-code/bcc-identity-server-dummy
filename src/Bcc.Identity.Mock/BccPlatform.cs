using BccCode.Core.Api.Client;

namespace Bcc.Identity.Mock;

public static class BccPlatformExtensions
{
    public static void AddBccPlatform(this WebApplicationBuilder builder)
    {
        // Configure BCC Core API client
        builder.Services.AddBccPlatform()
            .AddBccCoreApiClient(new CoreApiClientOptions
            {
                Scopes = [
                    CoreApiScope.PersonsRead,
                    CoreApiScope.PersonsPersonIdRead,
                    CoreApiScope.PersonsEmailRead,
                    CoreApiScope.PersonsNameRead,
                    CoreApiScope.PersonsPreferencesRead,
                    CoreApiScope.AffiliationsRead,
                    CoreApiScope.RelationsRead,
                    CoreApiScope.OrgsRead,
                    CoreApiScope.ConsentsRead,
                    CoreApiScope.ConsentsWrite,
                ],
                QueryOptions = new CoreApiQueryOptions
                {
                    // Only include persons with an active membership
                    IncludePersonsWithoutChurchAffiliation = false,
                }
            });
    }
}