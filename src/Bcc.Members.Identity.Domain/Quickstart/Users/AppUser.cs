using Microsoft.AspNetCore.Identity;

namespace Bcc.Members.Identity.Domain.Quickstart.Users
{
    public class AppUser : IdentityUser
    {
        public string Name { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DateOfBirth { get; set; }
        public string Password { get; set; }

        public string personId  {get ;set ;}
        public bool hasMembership { get; set; }

        public string churchId { get; set; }
        public string churchName { get; set; }

    }
}
