using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using signalrserver.HubConnection;
using vendtechext.BLL.DTO;

namespace vendtechext.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BalanceController : ControllerBase
    {
        private readonly ILogger<BalanceController> _logger;
        private readonly IHubContext<MessageHub, IMessageHub> hubContext;

        public BalanceController(ILogger<BalanceController> logger, IHubContext<MessageHub, IMessageHub> hubContext)
        {
            _logger = logger;
            this.hubContext = hubContext;
        }



        [HttpPost("update", Name = "update")]
        public IActionResult BalanceUpdate([FromBody] MessageBody request)
        {
            _logger.LogInformation(1, null, "This is it");
            hubContext.Clients.All.SendBalanceUpdate("BalanceUpdate", request.UserId);
            return Ok(request);
        }

    }
}