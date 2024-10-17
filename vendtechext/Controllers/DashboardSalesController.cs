using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using vendtechext.BLL.Interfaces;
using vendtechext.Contracts;
using vendtechext.Controllers.Base;

namespace vendtechext.Controllers
{
    [ApiController]
    [Route("sales/v1/")]
    [Authorize]
    public class DashboardSalesController : BaseController
    {
        private readonly ISalesService service;

        public DashboardSalesController(ILogger<APISalesController> logger, ISalesService salesService) : base(logger)
        {
            service = salesService;
        }

        [HttpPost("get-report")]
        public async Task<IActionResult> PurchaseElectricity([FromBody] PaginatedSearchRequest request)
        {
            APIResponse reponse = await service.GetSalesReportAsync(request);
            return Ok(reponse);
        }
    }
}
