using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using signalrserver.HubConnection;
using vendtechext.BLL.Interfaces;
using vendtechext.Contracts;

namespace vendtechext.Controllers
{
    [ApiController]
    [Route("vendtech")]
    public class VendtechWebSignalsController : ControllerBase
    {
        private readonly ILogger<VendtechWebSignalsController> _logger;
        private readonly IHubContext<CustomersHub, IMessageHub> customerhubContext;
        private readonly IHubContext<AdminHub, IMessageHub> adminHubContext;
        private readonly IMobilePushService _pushService;

        public VendtechWebSignalsController(ILogger<VendtechWebSignalsController> logger, IHubContext<CustomersHub, IMessageHub> customerhubContext, IHubContext<AdminHub, IMessageHub> adminHubContext, IMobilePushService pushService)
        {
            _logger = logger;
            this.customerhubContext = customerhubContext;
            this.adminHubContext = adminHubContext;
            _pushService = pushService;
        }



        [HttpPost("balance", Name = "balance")]
        public IActionResult BalanceUpdate([FromBody] MessageBody request)
        {
            customerhubContext.Clients.All.SendBalanceUpdate(request.UserId);
            return Ok(request);
        }

        [HttpPost("updatewigdetsales", Name = "updatewigdetsales")]
        public IActionResult UpdateWigdetSales([FromBody] MessageBody request)
        {
            adminHubContext.Clients.All.UpdateWigdetSales(request.Message);
            return Ok(request);
        }

        [HttpPost("updatewigdetdeposit", Name = "updatewigdetdeposit")]
        public IActionResult Updatewigdetdeposit([FromBody] MessageBody request)
        {
            adminHubContext.Clients.All.UpdateWigdetDeposits(request.Message);
            return Ok(request);
        }

        [HttpPost("updateunreleaseddeposit", Name = "updateunreleaseddeposit")]
        public IActionResult Updateunreleaseddeposit([FromBody] MessageBody request)
        {
            adminHubContext.Clients.All.UpdateAdminUnreleasedDeposits(request.Message);
            return Ok(request);
        }

        [HttpPost("admin_notification_count", Name = "admin_notification_count")]
        public IActionResult AdminNotificationCount([FromBody] MessageBody request)
        {
            adminHubContext.Clients.All.UpdateAdminNotificationCount(request.Message);
            return Ok(request);
        }

        [HttpPost("push_to_mobile", Name = "push_to_mobile")]
        public async Task<IActionResult> PushMessageToMobileAsync([FromBody] MessageRequest request)
        {
            await _pushService.Push(request);
            return Ok("message sent successfully!");
        }
    }
}