using Microsoft.AspNetCore.Mvc;
using vendtechext.BLL.Interfaces;
using vendtechext.Contracts;
using vendtechext.Controllers.Base;
using vendtechext.DAL.Seed;
using vendtechext.Helper;

namespace vendtechext.Controllers
{
    [ApiController]
    [Route("lookups/v1")]
    public class LookupController : BaseController
    {
        private readonly IDepositService _depositService;
        private readonly ISalesService _salesService;
        public LookupController(ILogger<BaseController> logger, IDepositService depositService, ISalesService salesService) : base(logger)
        {
            _depositService = depositService;
            _salesService = salesService;
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
            Response<object> Response = new Response<object>();
            return Ok(Response.WithStatus("success").WithMessage("Successfully fetched").WithType(result).GenerateResponse());
        }

        [HttpPost("get-transaction-requestresponse")]
        public async Task<IActionResult> GetTransaction([FromBody] SingleTransation request)
        {
            var result = await _salesService.GetSingleTransactionAsync(request);
            return Ok(result);
        }

        [HttpGet("navigations")]
        public IActionResult nav()
        {
            var nav = new NavigationService();
            Response<object> Response = new Response<object>();
            return Ok(Response.WithStatus("success").WithMessage("Successfully fetched").WithType(nav.GetNavigationItems()).GenerateResponse());
        }
    }
}