using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using vendtechext.BLL.Interfaces;
using vendtechext.Contracts;

namespace vendtechext.Controllers
{
    [ApiController]
    [Route("auth/v1/")]
    public class AuthenticationController : ControllerBase
    {
        private readonly ILogger<AuthenticationController> _logger;
        private readonly IAuthService _service;

        public AuthenticationController(ILogger<AuthenticationController> logger, IAuthService authService)
        {
            _logger = logger;
            _service = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto request)
        {
            var result = await _service.LoginAsync(request);
            return Ok(result);
        }
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto request)
        {
            var result = await _service.RefreshTokenAsync(request);
            return Ok(result);
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ChangeForgottenPassword([FromBody] ForgotPassword request)
        {
            var result = await _service.ChangeForgottenPassword(request.AppUserId, request.Token, request.NewPassword);
            return Ok(result);
        }

        [HttpPost("generate-password-reset-token")]
        public async Task<IActionResult> GenerateResetLink([FromBody] ResetToken request)
        {
            var result = await _service.GeneratePasswordResetToken(request.Email);
            return Ok(result);
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePsssword([FromBody] ChangePassword request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _service.ChangePassword(userId, request.OldPassword, request.NewPassword);
            return Ok(result);
        }

        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _service.GetProfileAsync(userId);
            return Ok(result);
        }

        [HttpPost("update-admin-account")]
        public async Task<IActionResult> UpdateUser([FromForm] AdminAccount request)
        {
            var result = await _service.UpdateAdminAccount(request);
            return Ok(result);
        }
    }
}