using Microsoft.AspNetCore.Identity;

namespace Bcc.Members.Identity.Domain.Quickstart.Users
{
    public class AppUser : IdentityUser
    {
        public string Name { get; set; }
        public string DateOfBirth { get; set; }
        public string Password { get; set; }
    }
}
