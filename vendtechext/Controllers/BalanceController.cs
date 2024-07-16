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
        private readonly IHubContext<CustomersHub, IMessageHub> hubContext;

        public BalanceController(ILogger<BalanceController> logger, IHubContext<CustomersHub, IMessageHub> hubContext)
        {
            _logger = logger;
            this.hubContext = hubContext;
        }



        [HttpPost("update", Name = "update")]
        public IActionResult BalanceUpdate([FromBody] MessageBody request)
        {
            _logger.LogInformation(1, null, request.ToString());
            hubContext.Clients.All.SendBalanceUpdate(request.UserId);
            return Ok(request);
        }

    }
}