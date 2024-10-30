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

        public AdminSalesController(ISalesService service)
        {
            _service = service;
        }

        [HttpPost("get-all")]
        public async Task<IActionResult> Get([FromBody] PaginatedSearchRequest request)
        {
            var result = await _service.GetSalesReportAsync(request);
            return Ok(result);
        }
    }
}