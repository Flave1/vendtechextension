using Microsoft.AspNetCore.Http;
using vendtechext.DAL.Common;
using vendtechext.DAL.Models;

namespace vendtechext.Contracts
{
    public class RegisterDto
    {
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Username { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public UserType UserType { get; set; }
        public int CommissionLevel { get; set; }
        public IFormFile image { get; set; }
    }
    public class ChangePassword
    {
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }
    public class ForgotPassword
    {
        public string AppUserId { get; set; }
        public string Token { get; set; }
        public string NewPassword { get; set; }
    }


    public class ResetToken
    {
        public string Email { get; set; }
    }

    public class RefreshTokenDto
    {
        public string UserId { get; set; }
        public string RefreshToken { get; set; }
    }

    public class LoginDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class AuthResponse
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }

    public class ProfileDto
    {
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Email { get; set; }
        public string BusinessName { get; set; }
        public string Id { get; set; }
        public int UserType { get; set;}
        public string Description { get; set; }
        public string Phone { get; set; }
        public string ApiKey { get; set; }
        public string Logo { get; set; }
        public ProfileDto(AppUser x, string businessName, string description, string apiKey, string logo = "")
        {

            Firstname = x.FirstName;
            Lastname = x.LastName;
            Email = x.Email;
            BusinessName = businessName;
            Id = x.Id;
            UserType = x.UserType;
            Description = description;
            Phone = x.PhoneNumber;
            ApiKey = apiKey;
            Logo = logo;
        }

    }

    public class AdminAccount
    {
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string AppUserId { get; set; }
        public IFormFile image { get; set; }
    }
}
