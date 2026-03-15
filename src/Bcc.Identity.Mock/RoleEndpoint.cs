using BccCode.Core.Api.Client;
using BccCode.Linq.Client;

namespace Bcc.Identity.Mock;

public static class RoleEndpoint
{
    public static void MapUserRoleEndpoint(this WebApplication app)
    {
        app.MapGet("persons", GetOrganisationRoles);
    }

    public static async Task<List<Organization>?> GetOrganisationRoles(ICoreApiClient client)
    {
        var persons = await client.Persons.Query()
            .Where(x=>x.IsActive == true)
            .Where(x=>x.Email != "")
            .Select(x => new
            {
                x.DisplayName,
                x.Email,
                x.Uid,
                x.Church,
            })
            .ToListAsync();

        return persons.GroupBy(x => x.Church.Name)
            .Select(x => new Organization()
            {
                Name = x.Key,
                Persons = x.Select(p => new Person(p.Email, p.DisplayName, p.Uid)).ToArray(),
            })
            .ToList();
    }
}

public class Organization
{
    public required string Name { get; set; }
    public required Person[] Persons { get; set; }
}

public record Person(string Email, string Name, Guid PersonUid);