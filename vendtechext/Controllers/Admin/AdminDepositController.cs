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
        private readonly IDepositService _depositService;
        private readonly IHttpContextAccessor _contextAccessor;

        public AdminDepositController(IDepositService depositService, IHttpContextAccessor contextAccessor)
        {
            _depositService = depositService;
            _contextAccessor = contextAccessor;
        }

        [HttpPost("aprrove")]
        public async Task<IActionResult> Create([FromBody] ApproveDepositRequest request)
        {
            var result = await _depositService.ApproveDeposit(request);
            return Ok(result);
        }

        [HttpGet("get-pending")]
        public async Task<IActionResult> GetPaymentTypes()
        {
            var result = await _depositService.GetPendingDeposits();
            return Ok(result);
        }

        [HttpGet("get")]
        public async Task<IActionResult> Get(Guid integrator_id)
        {
            var result = await _depositService.GetIntegratorDeposits(integrator_id);
            return Ok(result);
        }

        
    }
}