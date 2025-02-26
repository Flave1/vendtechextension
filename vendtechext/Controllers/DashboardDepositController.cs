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
        private readonly IDepositService _service;
        private readonly IHttpContextAccessor _contextAccessor;

        public DashboardDepositController(IDepositService depositService, IHttpContextAccessor contextAccessor)
        {
            _service = depositService;
            _contextAccessor = contextAccessor;
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] DepositRequest request)
        {
            var integrator_id = Guid.Parse(_contextAccessor?.HttpContext?.User?.FindFirst(r => r.Type == "integrator_id")?.Value?? "");
            var result = await _service.CreateDeposit(request, integrator_id);
            return Ok(result);
        }
        [HttpPost("get")]
        public async Task<IActionResult> GetDeposits([FromBody] PaginatedSearchRequest request)
        {
            var integrator_id = Guid.Parse(_contextAccessor?.HttpContext?.User?.FindFirst(r => r.Type == "integrator_id")?.Value ?? "");
            request.IntegratorId = integrator_id;
            var result = await _service.GetIntegratorDeposits(request);
            return Ok(result);
        }


    }
}