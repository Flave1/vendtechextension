using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using vendtechext.BLL.Services;
using vendtechext.Contracts;

namespace vendtechext.Controllers
{
    [ApiController]
    [Route("notification/v1/")]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly NotificationHelper _service;
        private readonly IHttpContextAccessor _contextAccessor;

        public NotificationController(NotificationHelper service, IHttpContextAccessor contextAccessor)
        {
            _service = service;
            _contextAccessor = contextAccessor;
        }

        [HttpPost("update")]
        public IActionResult Create([FromBody] NotificationDtoUpdate request)
        {
            var receiver = _contextAccessor?.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value?? "";
           _service.UpdateNotificationReadStatus(request.Id, receiver);
            return Ok(receiver);
        }
        [HttpGet("get")]
        public  IActionResult Get()
        {
            var receiver = _contextAccessor?.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            var nots = _service.GetNotifications(receiver);
            return Ok(nots);
        }

        [HttpGet("get-single")]
        public IActionResult Get(long id)
        {
            var nots = _service.GetNotification(id);
            return Ok(nots);
        }
    }
}