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
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool Deleted { get; set; }
        public string PinCode { get; set; }
        public string DeviceToken { get; set; }
        public bool IsPinNew { get; set; } = true;
        public string AppVersion { get; set; }
        public string MigrationUniqueId { get; set; }
    }
}
