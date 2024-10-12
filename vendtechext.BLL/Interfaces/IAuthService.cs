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
    }
}
