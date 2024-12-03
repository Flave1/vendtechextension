using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using vendtechext.BLL.Interfaces;
using vendtechext.Contracts;
using vendtechext.Controllers.Base;

namespace vendtechext.Controllers
{
    [ApiController]
    [Route("integrator/v1")]
    [Authorize]
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


        [HttpPost("update")]
        public async Task<IActionResult> UpdateBusinessUser([FromForm] BusinessUserDTO businessUser)
        {
            var result = await service.UpdateBusinessAccount(businessUser);
            return Ok(result);
        }

        [HttpGet("get-wallet-balance")]
        public async Task<IActionResult> GetBalance(bool includeLastDeposit)
        {
            if (User.IsInRole("Integrator"))
            {
                Guid integrator_id = Guid.Parse(_contextAccessor?.HttpContext?.User?.FindFirst(r => r.Type == "integrator_id")?.Value ?? "");
                var result = await _depositService.GetWalletBalance(integrator_id, includeLastDeposit);
                return Ok(result);
            }
            else
            {
                var result = await _depositService.GetAdminBalance();
                return Ok(result);
            }
        }

        [HttpGet("get-today-activity")] 
        public IActionResult GetTodayActivity()
        {
            if (User.IsInRole("Integrator"))
            {
                Guid integrator_id = Guid.Parse(_contextAccessor?.HttpContext?.User?.FindFirst(r => r.Type == "integrator_id")?.Value ?? "");
                var result = _depositService.GetTodaysTransaction(integrator_id);
                return Ok(result);
            }
            else
            {
                var result = _depositService.GetAdminTodaysTransaction();
                return Ok(result);
            }

        }
    }
}