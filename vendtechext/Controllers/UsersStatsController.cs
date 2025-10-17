using Microsoft.AspNetCore.Mvc;
using vendtechext.BLL.Interfaces;
using vendtechext.Controllers.Base;
using vendtechext.Helper;

namespace vendtechext.Controllers
{
    [ApiController]
    [Route("users-stats/v1/")]
    public class UsersStatsController : BaseController
    {
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly NotificationService _notificationService;

        public UsersStatsController(ILogger<UsersStatsController> logger, IHttpContextAccessor contextAccessor, NotificationService notificationService) : base(logger)
        {
            _contextAccessor = contextAccessor;
            _notificationService = notificationService;
        }
        [HttpGet("notification-count")]
        public IActionResult NotificationCount()
        {
            string userId = _contextAccessor?.HttpContext?.User?.FindFirst(r => r.Type == "user_id")?.Value ?? "";
            var result = _notificationService.GetNotificationCount(userId);
            return Ok(new {CounResult = result  });
        }
       
    }
}
