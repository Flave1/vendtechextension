using Microsoft.AspNetCore.Identity;

namespace vendtechext.DAL.Models
{
    public class AppUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int UserType { get; set; }
        public int UserAccountStatus { get; set; } = 1;
    }
}
