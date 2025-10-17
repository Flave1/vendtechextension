using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using vendtechext.BLL.Interfaces;
using vendtechext.BLL.Services;
using vendtechext.Contracts;
using vendtechext.DAL.Models;
using vendtechext.Helper;
using vendtechext.SDK.HubConnection;
using vendtechext.SDK.Models;

namespace vendtechext.Controllers
{
    [ApiController]
    [Route("notification/v1/")]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly NotificationService _service;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IHubContext<CustomNotificationHub, ICustomNotificationHub> _customHubContext;
        private readonly IAuthService _authService;
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly IHubContext<CustomNotificationHub> _dynamicHubContext;

        public NotificationController(NotificationService service, IHttpContextAccessor contextAccessor, IHubContext<CustomNotificationHub, ICustomNotificationHub> customHubContext, IAuthService authService, IBackgroundJobClient backgroundJobClient, IHubContext<CustomNotificationHub> dynamicHubContext)
        {
            _service = service;
            _contextAccessor = contextAccessor;
            _customHubContext = customHubContext;
            _authService = authService;
            _backgroundJobClient = backgroundJobClient;
            _dynamicHubContext = dynamicHubContext;
        }

        [HttpPost("update")]
        public IActionResult Create([FromBody] NotificationDtoUpdate request)
        {
            var receiver = _contextAccessor?.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            _service.UpdateNotificationReadStatus(request.Id, receiver);
            return Ok(receiver);
        }
        [HttpGet("get")]
        public IActionResult Get()
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

        [AllowAnonymous]
        [HttpPost("create-event")]
        public async Task<IActionResult> CreateEvent([FromBody] EventRequest request)
        {

            await _dynamicHubContext.Clients
                .Group(request.EventReceiver)
                .SendAsync(request.EventName, request.EventValue);

            return Ok(request);
        }

        [AllowAnonymous]
        [HttpPost("create-notification")]
        public async Task<IActionResult> CreateNotification([FromBody] NotificationRequest request)
        {
            if (request is null) return BadRequest("Request body is required.");

            AppUser? user = null;

            if (request.ToAdmin)
            {
                var admins = await _authService.FindAdminUser();
                user = admins.FirstOrDefault();
                request.UserId = user?.Id;
            }

            if (string.IsNullOrWhiteSpace(request.Email) && !string.IsNullOrWhiteSpace(request.UserId))
            {
                user ??= await _authService.FindUserById(request.UserId);
                if (user == null)
                    return BadRequest($"User '{request.UserId}' not found.");

                request.Email = user.Email;
                if (string.IsNullOrWhiteSpace(request.FirstName))
                    request.FirstName = user.FirstName;
            }

            if (string.IsNullOrWhiteSpace(request.Email))
                return BadRequest("Could not resolve recipient email.");

            var channels = new List<INotificationChannel>
            {
                new SmsNotificationChannel(),
                new EmailNotificationChannel(),
                new PushNotificationChannel(),
                new DatabaseNotificationChannel()
            };

            var service = new NotificationService(channels);
            service.SendNotification(request);

            return Ok(new NotificationResponse(true, "Sent"));
        }

        [HttpPost("success")]
        public async Task<IActionResult> SuccessNotification([FromBody] MessageBody request)
        {
            await _customHubContext.Clients.Group(request.UserId).SuccessNotificationCreated(request.Message);
            return Ok();
        }

        [HttpPost("failed")]
        public async Task<IActionResult> FailedNotification([FromBody] MessageBody request)
        {
            await _customHubContext.Clients.Group(request.UserId).FailedNotificationCreated(request.Message);
            return Ok();
        }

        [HttpPost("warning")]
        public async Task<IActionResult> WarningNotification([FromBody] MessageBody request)
        {
            await _customHubContext.Clients.Group(request.UserId).WarningNotificationCreated(request.Message);
            return Ok();
        }

        [HttpPost("info")]
        public async Task<IActionResult> InfoNotification([FromBody] MessageBody request)
        {
            await _customHubContext.Clients.Group(request.UserId).InfoNotificationCreated(request.Message);
            return Ok();
        }
        [HttpPost("notify-admin")]
        public async Task<IActionResult> NotifyAdmin([FromBody] MessageBody request)
        {
            IList<AppUser> users = await _authService.FindAdminUser();
            for (int i = 0; i < users.Count; i++)
            {
                await _customHubContext.Clients.Group(users[i].Id).NotifyAdmins(request.Message);
            }
            return Ok();
        }
    }
}