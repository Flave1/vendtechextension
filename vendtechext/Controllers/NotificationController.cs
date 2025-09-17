using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using vendtechext.BLL.HubConnection;
using vendtechext.BLL.Interfaces;
using vendtechext.BLL.Services;
using vendtechext.Contracts;
using vendtechext.DAL.Models;

namespace vendtechext.Controllers
{
    [ApiController]
    [Route("notification/v1/")]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly NotificationService _service;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IHubContext<CustomNotificationHub, ICustomNotificationHub> _integratorHubContext;
        private readonly IAuthService _authService;
        private readonly IBackgroundJobClient _backgroundJobClient;

        public NotificationController(NotificationService service, IHttpContextAccessor contextAccessor, IHubContext<CustomNotificationHub, ICustomNotificationHub> integratorHubContext, IAuthService authService, IBackgroundJobClient backgroundJobClient)
        {
            _service = service;
            _contextAccessor = contextAccessor;
            _integratorHubContext = integratorHubContext;
            _authService = authService;
            _backgroundJobClient = backgroundJobClient;
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

        [HttpPost("create-notification")]
        public IActionResult CreateNotification([FromBody] NotificationRequest request)
        {
            var channels = new List<INotificationChannel>
            {
                new SmsNotificationChannel(),
                new EmailNotificationChannel(),
                new PushNotificationChannel(),
                new DatabaseNotificationChannel()
             };

            var service = new NotificationService(channels);

            _backgroundJobClient.Enqueue(() => service.SendNotification(request));
            
            return Ok();
        }

        [HttpPost("success")]
        public async Task<IActionResult> SuccessNotification([FromBody] MessageBody request)
        {
            await _integratorHubContext.Clients.Group(request.UserId).SuccessNotificationCreated(request.Message);
            return Ok();
        }

        [HttpPost("failed")]
        public async Task<IActionResult> FailedNotification([FromBody] MessageBody request)
        {
            await _integratorHubContext.Clients.Group(request.UserId).FailedNotificationCreated(request.Message);
            return Ok();
        }

        [HttpPost("warning")]
        public async Task<IActionResult> WarningNotification([FromBody] MessageBody request)
        {
            await _integratorHubContext.Clients.Group(request.UserId).WarningNotificationCreated(request.Message);
            return Ok();
        }

        [HttpPost("info")]
        public async Task<IActionResult> InfoNotification([FromBody] MessageBody request)
        {
            await _integratorHubContext.Clients.Group(request.UserId).InfoNotificationCreated(request.Message);
            return Ok();
        }
        [HttpPost("notify-admin")]
        public async Task<IActionResult> NotifyAdmin([FromBody] MessageBody request)
        {
            IList<AppUser> users = await _authService.FindAdminUser();
            for (int i = 0; i < users.Count; i++)
            {
                await _integratorHubContext.Clients.Group(users[i].Id).NotifyAdmins(request.Message);
            }
            return Ok();
        }
    }
}