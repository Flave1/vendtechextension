using Microsoft.AspNetCore.Identity;

namespace vendtechext.DAL.Models
{
    public class AppUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int UserType { get; set; }
        public int UserAccountStatus { get; set; } = 1;
        public string ProfilePic { get; set; }
        public int CountryId { get; set; }
        public int CityId { get; set; }
        public string Address { get; set; }
    }
}
