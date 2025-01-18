using Microsoft.AspNetCore.Mvc;
using vendtechext.BLL.Interfaces;
using vendtechext.BLL.Middlewares;
using vendtechext.Contracts;
using vendtechext.Controllers.Base;
using vendtechext.DAL.Common;
using vendtechext.Helper;

namespace vendtechext.Controllers
{
    [ApiController]
    [Route("sales/v1/")]
    [EndpointValidator]
    public class APISalesController : BaseController
    {
        private readonly IAPISalesService service;
        private LogService _log;

        public APISalesController(ILogger<APISalesController> logger, IAPISalesService salesService, LogService log) : base(logger)
        {
            service = salesService;
            _log = log;
        }

        [HttpPost("buy")]
        public async Task<IActionResult> PurchaseElectricity([FromBody] ElectricitySaleRequest request)
        {
            var integratorId = Guid.Parse(HttpContext.Items["IntegratorId"] as string ?? "");
            var integratorName = HttpContext.Items["IntegratorName"] as string;

            _log.Log(LogType.Infor, $"received request for {request.TransactionId} from {integratorName}", request);
            APIResponse reponse = await service.PurchaseElectricity(request, integratorId, integratorName);
            //APIResponse reponse = await service.PurchaseElectricityForSandbox(request, integratorId, integratorName);
            _log.Log(LogType.Infor, $"response sent for {request.TransactionId} to {integratorName}", reponse);

            return Ok(reponse);
        }

        [HttpPost("status")]
        public async Task<IActionResult> SaleStatus([FromBody] SaleStatusRequest request)
        {
            var integratorId = Guid.Parse(HttpContext.Items["IntegratorId"] as string ?? "");
            var integratorName = HttpContext.Items["IntegratorName"] as string;

            APIResponse reponse = await service.QuerySalesStatus(request, integratorId, integratorName);

            return Ok(reponse);
        }
    }
}
