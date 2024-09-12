using Microsoft.AspNetCore.Mvc;
using vendtechext.BLL.DTO;
using vendtechext.BLL.Interfaces;
using vendtechext.BLL.Middlewares;

namespace vendtechext.Controllers
{
    [ApiController]
    [Route("sales/v1/")]
    [EndpointValidator]
    public class ElectricitySalesController : BaseController
    {
        private readonly IRTSSalesService service;

        public ElectricitySalesController(ILogger<ElectricitySalesController> logger, IRTSSalesService salesService): base(logger)
        {
            this.service = salesService;
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
            _logger.LogInformation(1, null, "");
            return await Task.Run(() => Ok(null));
        }

        [HttpPost("buy")]
        public async Task<IActionResult> PurchaseElectricity([FromBody] ElectricitySaleRequest request)
        {
            var integratorId = HttpContext.Items["IntegratorId"] as string;
            APIResponse reponse = await service.PurchaseElectricity(request, integratorId);
            return Ok(reponse);
        }
    }
}
