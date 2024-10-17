using vendtechext.DAL.Common;
using vendtechext.DAL.Models;

namespace vendtechext.Contracts
{
    public class RegisterDto
    {
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public UserType UserType { get; set; }
    }

    public class LoginDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class ProfileDto
    {
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Email { get; set; }
        public string BusinessName { get; set; }
        public string Id { get; set; }
        public int UserType { get; set;}
        public ProfileDto(AppUser x, string businessName) {

            Firstname = x.FirstName;
            Lastname = x.LastName;
            Email = x.Email;
            BusinessName = businessName;
            Id = x.Id;
            UserType = x.UserType;
        }

    }
}
