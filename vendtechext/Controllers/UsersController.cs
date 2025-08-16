using Microsoft.AspNetCore.Mvc;
using vendtechext.BLL.Interfaces;
using vendtechext.Contracts;
using vendtechext.Controllers.Base;
using vendtechext.DAL.Common;

namespace vendtechext.Controllers
{
    [ApiController]
    [Route("users/v1/")]
    public class UsersController: BaseController
    {
        private readonly IUsersService _service;

        public UsersController(ILogger<APISalesController> logger, IUsersService service): base(logger)
        {
            _service = service;
        }
        [HttpPost("create-agency-account")]
        public async Task<IActionResult> CreateAgencyAccount([FromForm] AgencyAccount request)
        {
            var result = await _service.CreateAgencyAccount(request);
            return Ok(result);
        }
        [HttpPost("update-agency-account/{userId}")]
        public async Task<IActionResult> UpdateAgencyAccount(string userId, [FromForm] AgencyAccount request)
        {
            var result = await _service.UpdateAgencyAccount(userId, request);
            return Ok(result);
        }

        [HttpGet("get-agency-account/{id}")]
        public async Task<IActionResult> GetAgencyAccount(string id)
        {
            var result = await _service.GetAgencyAccountById(id);
            return Ok(result);
        }

        [HttpPost("create-vendor-account")]
        public async Task<IActionResult> CreateVendorAccount([FromForm] VendorAccount request)
        {
            var result = await _service.CreateVendorAccount(request);
            return Ok(result);
        }
        [HttpPost("update-vendor-account/{userId}")]
        public async Task<IActionResult> UpdateVendorAccount(string userId, [FromForm] VendorAccount request)
        {
            var result = await _service.UpdateVendorAccount(userId, request);
            return Ok(result);
        }

        [HttpGet("get-vendor-account/{id}")]
        public async Task<IActionResult> GetVendorAccount(string id)
        {
            var result = await _service.GetVendorAccountById(id);
            return Ok(result);
        }

        [HttpGet("internal-use-get-agency-user-accounts")]
        public async Task<IActionResult> GetAgencyAccount()
        {
            var result = await _service.GetUserAccounts(UserType.Agency);
            return Ok(result);
        }
        [HttpGet("internal-use-get-vendor-user-accounts")]
        public async Task<IActionResult> GetVendorAccount()
        {
            var result = await _service.GetUserAccounts(UserType.Vendor);
            return Ok(result);
        }

        [HttpPost("update-vendor-passcode")]
        public async Task<IActionResult> UpdatePasscode([FromBody] UpdatePasscode request)
        {
            var result = await _service.UpdatePasscode(request);
            return Ok(result);
        }
    }
}
