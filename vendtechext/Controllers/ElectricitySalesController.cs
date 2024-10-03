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
    //[EndpointValidator]
    public class ElectricitySalesController : BaseController
    {
        private readonly IElectricitySalesService service;
        private ILogService _log;

        public ElectricitySalesController(ILogger<ElectricitySalesController> logger, IElectricitySalesService salesService, ILogService log) : base(logger)
        {
            service = salesService;
            _log = log;
        }

        //[HttpPost("json", ReceivedFrom = "json")]
        //public IActionResult ValidJson([FromBody] RTSRequestmodel request)
        //{
        //    _logger.LogInformation(1, null, "");
        //    return Ok(request);
        //}

        //[HttpPost("", ReceivedFrom = "")]
        //public async Task<IActionResult> PurchaseJson([FromBody] RTSRequestmodel request)
        //{
        //    _logger.LogInformation(1, null, "");
        //    return await Task.Run(() => Ok(null));
        //}

        [HttpPost("buy")]
        public async Task<IActionResult> PurchaseElectricity([FromBody] ElectricitySaleRequest request)
        {
            var integratorId = HttpContext.Items["IntegratorId"] as string ?? "c7d76941-5ed9-4961-59fc-08dcd3ecd192"; 
            var integratorName = HttpContext.Items["IntegratorName"] as string ?? "Test Business";

            _log.Log(LogType.Infor, $"received request for {request.TransactionId} from {integratorName}", request);
            APIResponse reponse = await service.PurchaseElectricity(request, integratorId, integratorName);
            _log.Log(LogType.Infor, $"response sent for {request.TransactionId} to {integratorName}", reponse);
            return Ok(reponse);
        }

        [HttpPost("status")]
        public async Task<IActionResult> SaleStatus([FromBody] SaleStatusRequest request)
        {
            var integratorId = HttpContext.Items["IntegratorId"] as string ?? "c7d76941-5ed9-4961-59fc-08dcd3ecd192";
            var integratorName = HttpContext.Items["IntegratorName"] as string ?? "Test Business";

            APIResponse reponse = await service.QuerySalesStatus(request, integratorId, integratorName);
            return Ok(reponse);
        }
    }
}
