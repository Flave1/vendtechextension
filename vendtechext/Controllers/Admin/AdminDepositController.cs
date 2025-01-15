using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using vendtechext.BLL.Interfaces;
using vendtechext.Contracts;

namespace vendtechext.Controllers
{
    [ApiController]
    [Route("admin-deposit/v1/")]
    [Authorize]
    public class AdminDepositController : ControllerBase
    {
        private readonly IDepositService _service;
        private readonly IHttpContextAccessor _contextAccessor;

        public AdminDepositController(IDepositService service, IHttpContextAccessor contextAccessor)
        {
            _service = service;
            _contextAccessor = contextAccessor;
        }

        [HttpPost("aprrove")]
        public async Task<IActionResult> ApproveDeposit([FromBody] ApproveDepositRequest request)
        {
            var currentUserid = _contextAccessor?.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            if (currentUserid != null) request.ApprovingUserId = currentUserid;
            var result = await _service.ApproveDeposit(request);
            return Ok(result);
        }

        [HttpPost("get-pending")]
        public async Task<IActionResult> GetPendingDeposits([FromBody] PaginatedSearchRequest request)
        {
            var result = await _service.GetPendingDeposits(request);
            return Ok(result);
        }

        [HttpPost("get-all")]
        public async Task<IActionResult> Get([FromBody] PaginatedSearchRequest request)
        {
            var result = await _service.GetIntegratorDeposits(request);
            return Ok(result);
        }
    }
}