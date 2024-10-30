using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        public AdminDepositController(IDepositService service)
        {
            _service = service;
        }

        [HttpPost("aprrove")]
        public async Task<IActionResult> Create([FromBody] ApproveDepositRequest request)
        {
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