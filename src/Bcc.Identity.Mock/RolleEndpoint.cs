using System.Text.Json.Serialization;

namespace Bcc.Identity.Mock;

public static class RolleEndpoint
{
    public static void MapUserRoleEndpoint(this WebApplication app)
    {
        app.MapGet("roles", GetOrganizationRoles);
    }

    public static Organization[] GetOrganizationRoles()
    {
        return [
            new Organization
            {
                Name = "Org 1",
                Roles =
                [
                    new Role("user1@bcc.no", "User 1"),
                    new Role("user2@bcc.no", "User 2")
                ]
            },
            new Organization
            {
                Name = "Org 2",
                Roles =
                [
                    new Role("user3@bcc.no", "User 3"),
                    new Role("user4@bcc.no", "User 4")
                ]
            }
        ];
    }
}

public class Organization
{
    public required string Name { get; set; }
    public required Role[] Roles { get; set; }
}

public record Role(string Email, string Name);