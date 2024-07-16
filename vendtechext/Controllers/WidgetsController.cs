using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using signalrserver.HubConnection;
using vendtechext.BLL.DTO;

namespace vendtechext.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WidgetsController : ControllerBase
    {
        private readonly ILogger<WidgetsController> _logger;
        private readonly IHubContext<AdminHub, IMessageHub> hubContext;

        public WidgetsController(ILogger<WidgetsController> logger, IHubContext<AdminHub, IMessageHub> hubContext)
        {
            _logger = logger;
            this.hubContext = hubContext;
        }



        [HttpPost("updatewigdetsales", Name = "updatewigdetsales")]
        public IActionResult UpdateWigdetSales([FromBody] MessageBody request)
        {
            _logger.LogInformation(1, null, request.Message);
            hubContext.Clients.All.UpdateWigdetSales(request.Message);
            return Ok(request);
        }

        [HttpPost("sales", Name = "sales")]
        public IActionResult sales([FromBody] MessageBody request)
        {
            _logger.LogInformation(1, null, request.Message);
            hubContext.Clients.All.SendBalanceUpdate(request.UserId);
            return Ok(request);
        }

        [HttpPost("deposits", Name = "deposits")]
        public IActionResult deposits([FromBody] MessageBody request)
        {
            _logger.LogInformation(1, null, "This is it");
            hubContext.Clients.All.SendBalanceUpdate(request.UserId);
            return Ok(request);
        }

        [HttpPost("walletbalance", Name = "walletbalance")]
        public IActionResult walletbalance([FromBody] MessageBody request)
        {
            _logger.LogInformation(1, null, "This is it");
            hubContext.Clients.All.SendBalanceUpdate(request.UserId);
            return Ok(request);
        }
    }
}