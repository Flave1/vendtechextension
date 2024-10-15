using Microsoft.AspNetCore.Mvc;
using vendtechext.BLL.Interfaces;
using vendtechext.Contracts;
using vendtechext.Controllers.Base;

namespace vendtechext.Controllers
{
    [ApiController]
    [Route("integrator/v1")]
    public class IntegratorController : BaseController
    {
        private readonly IIntegratorService service;
        private readonly IDepositService _depositService;
        private readonly IHttpContextAccessor _contextAccessor;
        public IntegratorController(ILogger<BaseController> logger, IIntegratorService b2bAccountService, IDepositService depositService, IHttpContextAccessor contextAccessor) : base(logger)
        {
            service = b2bAccountService;
            _depositService = depositService;
            _contextAccessor = contextAccessor;
        }


        [HttpPost("create")]
        public async Task<IActionResult> CreateBusinessUser([FromBody] BusinessUserCommandDTO businessUser)
        {
            await service.CreateBusinessAccount(businessUser);
            return Ok(businessUser);
        }

        [HttpPost("update")]
        public async Task<IActionResult> UpdateBusinessUser([FromBody] BusinessUserDTO businessUser)
        {
            await service.UpdateBusinessAccount(businessUser);
            return Ok(businessUser);
        }

        [HttpGet("get-wallet-balance")]
        public async Task<IActionResult> GetBalance(bool includeLastDeposit)
        {
            var integrator_id = Guid.Parse(_contextAccessor?.HttpContext?.User?.FindFirst(r => r.Type == "integrator_id")?.Value ?? "");
            var result = await _depositService.GetWalletBalance(integrator_id, includeLastDeposit);
            return Ok(result);
        }
    }
}