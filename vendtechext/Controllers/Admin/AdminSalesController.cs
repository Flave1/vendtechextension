using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using vendtechext.BLL.Interfaces;
using vendtechext.Contracts;

namespace vendtechext.Controllers
{
    [ApiController]
    [Route("admin-sales/v1/")]
    [Authorize]
    public class AdminSalesController : ControllerBase
    {
        private readonly ISalesService _service;
        private readonly IAPISalesService _aPISalesService;

        public AdminSalesController(ISalesService service, IAPISalesService aPISalesService)
        {
            _service = service;
            _aPISalesService = aPISalesService;
        }

        [HttpPost("get-all")]
        public async Task<IActionResult> Get([FromBody] PaginatedSearchRequest request)
        {
            var result = await _service.GetSalesReportAsync(request);
            return Ok(result);
        }


        [HttpPost("resolve-electricity-sale")]
        public async Task<IActionResult> ResolveSale([FromBody] SaleStatusRequest request, string integratorId, string integratorName)
        {

            var result = await _aPISalesService.QuerySalesStatus(request, Guid.Parse(integratorId), integratorName);
            return Ok(result);
        }


    }
}