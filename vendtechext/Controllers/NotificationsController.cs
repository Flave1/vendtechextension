using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using signalrserver.HubConnection;
using vendtechext.BLL.DTO;

namespace vendtechext.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class NotificationsController : ControllerBase
    {
        private readonly ILogger<NotificationsController> _logger;
        private readonly IHubContext<AdminHub, IMessageHub> hubContext;

        public NotificationsController(ILogger<NotificationsController> logger, IHubContext<AdminHub, IMessageHub> hubContext)
        {
            _logger = logger;
            this.hubContext = hubContext;
        }

        [HttpPost("admin_notification_count", Name = "admin_notification_count")]
        public IActionResult AdminNotificationCount([FromBody] MessageBody request)
        {
            _logger.LogInformation(1, null, request.Message);
            hubContext.Clients.All.UpdateAdminNotificationCount(request.Message);
            return Ok(request);
        }
    }
}