using Microsoft.AspNetCore.Identity;
using vendtechext.Contracts;
using vendtechext.DAL.Models;

namespace vendtechext.BLL.Interfaces
{
    public interface IAuthService
    {
        Task<IdentityResult> RegisterAsync(RegisterDto registerDto);
        Task<AppUser> RegisterAndReturnUserAsync(RegisterDto registerDto);
        Task<APIResponse> LoginAsync(LoginDto loginDto);
        Task<AppUser> FindUserByEmail(string email);
        Task<APIResponse> GetProfileAsync(string userId);
        Task<AppUser> FindUserById(string id);
        Task<AppUser> UpdateAndReturnUserAsync(RegisterDto registerDto, string appUserId);
        Task<APIResponse> ChangeForgottenPassword(string userId, string token, string newPassword);
        Task<APIResponse> GeneratePasswordResetToken(string email);
        Task<APIResponse> ChangePassword(string userId, string oldPassword, string newPassword);
        Task<APIResponse> RefreshTokenAsync(RefreshTokenDto request);
        Task<APIResponse> UpdateAdminAccount(AdminAccount model);
    }
}
