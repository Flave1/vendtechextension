using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using vendtechext.BLL.Interfaces;
using vendtechext.Contracts;

namespace vendtechext.Controllers
{
    [ApiController]
    [Route("deposit/v1/")]
    [Authorize]
    public class DashboardDepositController : ControllerBase
    {
        private readonly IDepositService _depositService;
        private readonly IHttpContextAccessor _contextAccessor;

        public DashboardDepositController(IDepositService depositService, IHttpContextAccessor contextAccessor)
        {
            _depositService = depositService;
            _contextAccessor = contextAccessor;
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] DepositRequest request)
        {
            var integrator_id = Guid.Parse(_contextAccessor?.HttpContext?.User?.FindFirst(r => r.Type == "integrator_id")?.Value?? "");
            var result = await _depositService.CreateDeposit(request, integrator_id);
            return Ok(result);
        }
        [HttpGet("get")]
        public async Task<IActionResult> Get()
        {
            var integrator_id = Guid.Parse(_contextAccessor?.HttpContext?.User?.FindFirst(r => r.Type == "integrator_id")?.Value ?? "");
            var result = await _depositService.GetIntegratorDeposits(integrator_id);
            return Ok(result);
        }
    }
}