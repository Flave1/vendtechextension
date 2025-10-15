using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using vendtechext.BLL.Interfaces;
using vendtechext.Contracts;
using vendtechext.Helper;
using vendtechext.SDK;
using vendtechext.SDK.HubConnection;

namespace vendtechext.Controllers
{
    [ApiController]
    [Route("deposit/v1/")]
    [Authorize]
    public class DashboardDepositController : ControllerBase
    {
        private readonly IDepositService _service;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly LogService _logService;

        private readonly IHubContext<CustomNotificationHub, ICustomNotificationHub> _customHubContext;
        public DashboardDepositController(IDepositService depositService, IHttpContextAccessor contextAccessor
            , IHubContext<CustomNotificationHub, ICustomNotificationHub> customHubContext
, LogService logService)
        {
            _service = depositService;
            _contextAccessor = contextAccessor;
            _customHubContext = customHubContext;
            _logService = logService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] DepositRequest request)
        {
            var integrator_id = Guid.Parse(_contextAccessor?.HttpContext?.User?.FindFirst(r => r.Type == "integrator_id")?.Value?? "");
            var result = await _service.CreateDeposit(request, integrator_id);
            return Ok(result);
        }
        [HttpPost("get")]
        public async Task<IActionResult> GetDeposits([FromBody] PaginatedSearchRequest request)
        {
            var integrator_id = Guid.Parse(_contextAccessor?.HttpContext?.User?.FindFirst(r => r.Type == "integrator_id")?.Value ?? "");
            request.IntegratorId = integrator_id;
            var result = await _service.GetIntegratorDeposits(request);
            return Ok(result);
        }



        [HttpPost("test_alert")]
        public async Task<IActionResult> Test([FromBody] DepositRequest request)
        {

            var user_id = _contextAccessor?.HttpContext?.User?.FindFirst(r => r.Type == "nameid")?.Value ?? 
                          _contextAccessor?.HttpContext?.User?.FindFirst(r => r.Type == "user_id")?.Value ?? "";
            
        
            // Test with ALL clients first to verify basic connection

            // Then test with SuccessNotificationCreated
            await _customHubContext.Clients.Group(user_id).SuccessNotificationCreated("Private Notification Received");
            await _customHubContext.Clients.Group(user_id).PendingDepositCreated("Pending Deposit Notification Received");
            await _customHubContext.Clients.Group(user_id).PendingSalesCreated("Pending Sales Notification Received");
            await _customHubContext.Clients.All.TestMessage("Broadcast Notification Received");
            _logService.Log(LogType.Infor, "SuccessNotificationCreated testing :_" + user_id);
            return Ok(new {message="Tested Service", status=200});
        }

    }
}