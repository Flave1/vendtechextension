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
        public string VendorName { get; set; }
        public int CountryId { get; set; } = 0;
        public int CityId { get; set; } = 0;
        public string Address { get; set; } = "";
        public string MigrationUniqueId { get; set; } = "";
        public string PinCode { get; set; } = "";
        public bool IsNewPin{ get; set; } = false;
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
        public int UserType { get; set; }
        public string Description { get; set; }
        public string Phone { get; set; }
        public string ApiKey { get; set; }
        public string SubApiKey { get; set; }
        public string Logo { get; set; }
        public int MidnightBalanceAlertSwitch { get; set; }
        public ProfileDto(AppUser x, string businessName, string description, string apiKey, string subApiKey = "", string logo = "", int midnightBalanceAlertSwitch = 0)
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
            SubApiKey = subApiKey;
            MidnightBalanceAlertSwitch = midnightBalanceAlertSwitch;
        }

    }

    public class AdminAccount
    {
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string AppUserId { get; set; }
        public string Address { get; set; }
        public int CountryId { get; set; }
        public int CityId { get; set; }
        public string VendorName { get; set; }
        public IFormFile image { get; set; }
    }

    public class AgencyAccount
    {
        public int Id;
        public Guid PosId;
        public string UserId;
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string AgencyName { get; set; }
        public string Description { get; set; }
        public int Status { get; set; }
        public string PosNumber { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public int CommissionLevelId { get; set; }
        public IFormFile image { get; set; }
        public string imgUrl { get; set; }
        public int CountryId { get; set; }
        public int CityId { get; set; }
        public string Address { get; set; }
    }

    public class VendorAccount
    {
        public string Id;
        public string PosId { get; set; }
        public string UserId;
        public int AgencyId { get; set; }
        public string AgencyName { get; set; }
        public string VendorName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Status { get; set; }
        public string PosNumber { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public int CommissionLevelId { get; set; }
        public int CountryId { get; set; }
        public int CityId { get; set; }
        public string Address { get; set; }
        public IFormFile image { get; set; }
        public string imgUrl { get; set; }
        public string MigrationUniqueId { get; set; } = "";
        public bool IsNewPin { get; set; } = false;
        public string PinCode { get; set; } = "";
        public string Password { get; set; } = "";
    }

    public class VendorCommand
    {
        public string PosId { get; set; }
        public string UserId { get; set; }
        public int Status { get; set; }
        public string PosNumber { get; set; }
        public int AgencyId { get; set; }
        public string VendorName { get; set; }
        public int CommissionLevelId { get; set; }
    }

    public class UserDto
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string ImagwUrl { get; set; }
    }

    public class PinLoginRequest
    {
        public string PinCode { get; set; }
        public string DeviceToken { get; set; }
        public string AppVersion { get; set; }
    }

    public class PinLoginResponse
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public bool IsNewPin { get; set; }
    }

    public class DeviceTokenModel
    {
        public string UserId { get; set; }
        public string DeviceToken { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }

    public class SetPinRequest
    {
        public string PinCode { get; set; }
        public string DeviceToken { get; set; }
        public string Email { get; set; }
    }

    public class RecoverPinRequest
    {
        public string Email { get; set; }
    }

    public class ValidatePinTokenRequest
    {
        public string Token { get; set; }
        public string Email { get; set; }
    }

    public class RecoverPinResponse
    {
        public string Message { get; set; }
        public bool Success { get; set; }
    }

    public class UpdatePasscode
    {
        public string Passcode { get; set; }
        public string UserId { get; set; }
        public string CellPhone { get; set; }
        public string Email { get; set; }
    }
}
