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

        private readonly IHubContext<CustomNotificationHub, ICustomNotificationHub> _customHubContext;
        public DashboardDepositController(IDepositService depositService, IHttpContextAccessor contextAccessor
            , IHubContext<CustomNotificationHub, ICustomNotificationHub> customHubContext
            )
        {
            _service = depositService;
            _contextAccessor = contextAccessor;
            _customHubContext = customHubContext;
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
            
            Console.WriteLine($"Test Alert: User ID = {user_id}");
            Console.WriteLine($"Available claims: {string.Join(", ", _contextAccessor?.HttpContext?.User?.Claims?.Select(c => $"{c.Type}={c.Value}") ?? new string[0])}");

            // Test with ALL clients first to verify basic connection
            await _customHubContext.Clients.All.TestMessage("Broadcast test message");

            // Test with TestMessage to group
            //await _customHubContext.Clients.Group(user_id).TestMessage("Group test message");

            // Then test with SuccessNotificationCreated
            await _customHubContext.Clients.Group(user_id).SuccessNotificationCreated("Deposit has just been created");

            return Ok($"Test completed for user: {user_id}");
        }

    }
}