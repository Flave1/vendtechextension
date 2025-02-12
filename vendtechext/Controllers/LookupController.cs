using Microsoft.AspNetCore.Mvc;
using vendtechext.BLL.Interfaces;
using vendtechext.Contracts;
using vendtechext.Controllers.Base;
using vendtechext.Helper;

namespace vendtechext.Controllers
{
    [ApiController]
    [Route("lookups/v1")]
    public class LookupController : BaseController
    {
        private readonly IDepositService _depositService;
        private readonly AppConfiguration config;
        public LookupController(ILogger<BaseController> logger, IDepositService depositService, AppConfiguration config) : base(logger)
        {
            _depositService = depositService;
            this.config = config;
        }

        [HttpGet("get-payment-type")]
        public async Task<IActionResult> GetPaymentTypes()
        {
            var result = await _depositService.GetPaymentTypes();
            return Ok(result);
        }


        [HttpGet("settings")]
        public IActionResult Settings()
        {
            var result = AppConfiguration.GetSettings();
            Response Response = new Response();
            return Ok(Response.WithStatus("success").WithMessage("Successfully fetched").WithType(result).GenerateResponse());
        }

    }
}