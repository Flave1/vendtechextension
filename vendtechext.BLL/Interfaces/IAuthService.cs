using Microsoft.AspNetCore.Identity;
using vendtechext.Contracts;

namespace vendtechext.BLL.Interfaces
{
    public interface IAuthService
    {
        Task<IdentityResult> RegisterAsync(RegisterDto registerDto);
        Task<string> LoginAsync(LoginDto loginDto);
    }
}
