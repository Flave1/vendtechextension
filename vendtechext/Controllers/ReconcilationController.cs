using Microsoft.AspNetCore.Mvc;
using vendtechext.BLL.Interfaces;
using vendtechext.Contracts;
using vendtechext.Contracts.VtchMainModels;
using vendtechext.Controllers.Base;
using vendtechext.Helper;

namespace vendtechext.Controllers
{
    [ApiController]
    [Route("reconcile/v1/")]
    public class ReconcilationController : BaseController
    {
        private readonly IVendtechReconcillationService service;
        private LogService _log;

        public ReconcilationController(ILogger<ReconcilationController> logger, IVendtechReconcillationService service, LogService log) : base(logger)
        {
            this.service = service;
            _log = log;
        }

        [HttpPost("start")]
        public async Task<IActionResult> start([FromBody] ReconcileRequest request)
        {
            await service.ProcessRefundsAsync();
            return Ok();
        }

      
    }
}
