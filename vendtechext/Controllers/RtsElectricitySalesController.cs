using Microsoft.AspNetCore.Mvc;
using vendtechext.BLL.DTO;
using vendtechext.BLL.Interfaces;

namespace vendtechext.Controllers
{
    [ApiController]
    [Route("edsa/v2/purchase")]
    public class RtsElectricitySalesController : ControllerBase
    {
        private readonly ILogger<RtsElectricitySalesController> _logger;
        private readonly IRTSSalesService salesService;

        public RtsElectricitySalesController(ILogger<RtsElectricitySalesController> logger, IRTSSalesService salesService)
        {
            _logger = logger;
            this.salesService = salesService;
        }

        [HttpPost("json", Name = "json")]
        public IActionResult ValidJson([FromBody] RTSRequestmodel request)
        {
            _logger.LogInformation(1, null, "");
            return Ok(request);
        }

        [HttpPost("", Name = "")]
        public async Task<IActionResult> PurchaseJson([FromBody] RTSRequestmodel request)
        {
            var result = await salesService.PurchaseElectricity(request);
            _logger.LogInformation(1, null, "");
            return Ok(result);
        }
    }
}
