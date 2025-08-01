using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using vendtechext.BLL.HubConnection;
using vendtechext.BLL.Interfaces;
using vendtechext.Contracts;

namespace vendtechext.Controllers
{
    [ApiController]
    [Route("deposit/v1/")]
    [Authorize]
    public class DashboardDepositController : ControllerBase
    {
        private readonly IDepositService _service;
        private readonly IHttpContextAccessor _contextAccessor;

        private readonly IHubContext<CustomNotificationHub, ICustomNotificationHub> _integratorHubContext;
        public DashboardDepositController(IDepositService depositService, IHttpContextAccessor contextAccessor
            , IHubContext<CustomNotificationHub, ICustomNotificationHub> integratorHubContext
            )
        {
            _service = depositService;
            _contextAccessor = contextAccessor;
            _integratorHubContext = integratorHubContext;
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

            var user_id = _contextAccessor?.HttpContext?.User?.FindFirst(r => r.Type == "user_id")?.Value ?? "";
            //await _integratorHubContext.Clients.User(user_id).DepositCreated("Deposit has just been created");
            //await _integratorHubContext.Clients.All.DepositCreated("Deposit has just been created");
            await _integratorHubContext.Clients.Group(user_id).SuccessNotificationCreated("Deposit has just been created");
            //await _integratorHubContext.Clients.Group(user_id).FailedNotificationCreated("Deposit has just been created");
            //await _integratorHubContext.Clients.Group(user_id).WarningNotificationCreated("Deposit has just been created");
            //await _integratorHubContext.Clients.Group(user_id).InfoNotificationCreated("Deposit has just been created");

            return Ok("Alright");
        }

    }
}