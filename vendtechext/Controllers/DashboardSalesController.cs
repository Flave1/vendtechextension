using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
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
        private readonly ISalesService _service;
        private readonly IHttpContextAccessor _contextAccessor;

        public DashboardSalesController(ILogger<DashboardSalesController> logger, ISalesService salesService, IHttpContextAccessor contextAccessor) : base(logger)
        {
            _service = salesService;
            _contextAccessor = contextAccessor;
        }

        [HttpPost("get-report")]
        public async Task<IActionResult> GetSalesReport([FromBody] PaginatedSearchRequest request)
        {

            var integrator_id = Guid.Parse(_contextAccessor?.HttpContext?.User?.FindFirst(r => r.Type == "integrator_id")?.Value ?? "");
            request.IntegratorId = integrator_id;
            APIResponse reponse = await _service.GetSalesReportAsync(request);
            return Ok(reponse);
        }

    }
}
