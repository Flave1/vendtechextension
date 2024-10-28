using Microsoft.AspNetCore.Mvc;
using vendtechext.BLL.Interfaces;
using vendtechext.Controllers.Base;

namespace vendtechext.Controllers
{
    [ApiController]
    [Route("lookups/v1")]
    public class LookupController : BaseController
    {
        private readonly IIntegratorService service;
        private readonly IDepositService _depositService;
        public LookupController(ILogger<BaseController> logger, IIntegratorService b2bAccountService, IDepositService depositService) : base(logger)
        {
            service = b2bAccountService;
            _depositService = depositService;
        }

        [HttpGet("get-payment-type")]
        public async Task<IActionResult> GetPaymentTypes()
        {
            var result = await _depositService.GetPaymentTypes();
            return Ok(result);
        }

    }
}